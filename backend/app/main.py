import os
from contextlib import asynccontextmanager
from fastapi import Depends, FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from sqlalchemy import desc, select, text
from sqlalchemy.ext.asyncio import AsyncSession
from .auth import (
    generate_access_token,
    get_current_user,
    hash_password,
    verify_password,
)
from .database import engine, get_db
from .models import Base, Follow, Post, User


# ===== App startup and configs =====
# Create tables on startup
@asynccontextmanager
async def lifespan(app: FastAPI):
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    try:
        yield
    finally:
        await engine.dispose()


app = FastAPI(lifespan=lifespan)

# CORS configuration
FRONTEND_ORIGIN = os.getenv("FRONTEND_ORIGIN")
if not FRONTEND_ORIGIN:
    raise RuntimeError("FRONTEND_ORIGIN is not set in environment variables.")
app.add_middleware(
    CORSMiddleware,
    allow_origins=[FRONTEND_ORIGIN],
    allow_methods=["*"],
    allow_headers=["*"],
)


# ===== DTOs =====
class RegisterIn(BaseModel):
    username: str
    password: str = Field(..., min_length=10, max_length=30)


class LoginIn(BaseModel):
    username: str
    password: str


# ===== Routes =====
@app.get("/hello")
def hello():
    return {"message": "welcome to lazybook"}


@app.post("/auth/register", status_code=201)
async def register(payload: RegisterIn, db: AsyncSession = Depends(get_db)):
    username = payload.username.strip()

    result = await db.execute(select(User).where(User.username == username))
    existing_user = result.scalars().first()

    if existing_user:
        raise HTTPException(status_code=409, detail="username already taken")

    user = User(username=username, password_hash=hash_password(payload.password))
    db.add(user)
    await db.commit()

    return {"success": True, "data": {"username": user.username}}


@app.post("/auth/login")
async def login(payload: LoginIn, db: AsyncSession = Depends(get_db)):
    result = await db.execute(select(User).where(User.username == payload.username))
    user = result.scalars().first()
    if not user or not verify_password(payload.password, user.password_hash):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED, detail="invalid credentials"
        )

    token = generate_access_token(user_id=user.id, username=user.username)

    return {"access_token": token, "token_type": "Bearer"}


@app.get("/users")
async def get_users(
    db: AsyncSession = Depends(get_db), me: User = Depends(get_current_user)
):
    result = await db.execute(select(User).order_by(User.id.asc()))
    users = result.scalars().all()

    return [{"id": user.id, "username": user.username} for user in users]


class UserOut(BaseModel):
    id: int
    username: str
    status: str


class Relationship(BaseModel):
    is_self: bool
    i_follow: bool
    follows_me: bool


class UserProfileOut(BaseModel):
    id: int
    username: str
    status: str
    followers_count: int
    following_count: int
    relationship: Relationship


class UserUpdateIn(BaseModel):
    status: str


@app.get("/me")
async def get_me(
    db: AsyncSession = Depends(get_db), me: User = Depends(get_current_user)
):
    followers_count = (
        await db.scalar(
            text("SELECT COUNT(*) FROM follows WHERE followee_id = :uid"),
            {"uid": me.id},
        )
        or 0
    )

    following_count = (
        await db.scalar(
            text("SELECT COUNT(*) FROM follows WHERE follower_id = :uid"),
            {"uid": me.id},
        )
        or 0
    )

    return UserProfileOut(
        id=me.id,
        username=me.username,
        status=me.status,
        followers_count=followers_count,
        following_count=following_count,
        relationship=Relationship(
            is_self=True,
            i_follow=False,
            follows_me=False,
        ),
    )


@app.get("/users/{user_id}")
async def get_user(
    user_id: int,
    db: AsyncSession = Depends(get_db),
    me: User = Depends(get_current_user),
):
    user = await db.get(User, user_id)
    if not user:
        raise HTTPException(status_code=404, detail="user not found")

    followers_count = (
        await db.scalar(
            text("SELECT COUNT(*) FROM follows WHERE followee_id = :uid"),
            {"uid": user_id},
        )
        or 0
    )

    following_count = (
        await db.scalar(
            text("SELECT COUNT(*) FROM follows WHERE follower_id = :uid"),
            {"uid": user_id},
        )
        or 0
    )

    is_self = user_id == me.id

    i_follow = bool(
        await db.scalar(
            text(
                "SELECT 1 FROM follows "
                "WHERE follower_id = :me AND followee_id = :followee_id LIMIT 1"
            ),
            {"me": me.id, "followee_id": user_id},
        )
    )

    follows_me = bool(
        await db.scalar(
            text(
                "SELECT 1 FROM follows "
                "WHERE follower_id = :followee_id AND followee_id = :me LIMIT 1"
            ),
            {"me": me.id, "followee_id": user_id},
        )
    )

    return UserProfileOut(
        id=user_id,
        username=user.username,
        status=user.status,
        followers_count=followers_count,
        following_count=following_count,
        relationship=Relationship(
            is_self=is_self,
            i_follow=i_follow,
            follows_me=follows_me,
        ),
    )


@app.put("/users/{user_id}")
async def update_user(
    user_id: int,
    payload: UserUpdateIn,
    db: AsyncSession = Depends(get_db),
    me: User = Depends(get_current_user),
):
    if me.id != user_id:
        raise HTTPException(status_code=403, detail="cannot update another user")

    user = await db.get(User, user_id)
    if not user:
        raise HTTPException(status_code=404, detail="user not found")

    user.status = payload.status
    await db.commit()
    await db.refresh(user)

    return UserOut(id=user.id, username=user.username, status=user.status)


@app.post("/users/{user_id}/follow", status_code=201)
async def follow(
    user_id: int,
    db: AsyncSession = Depends(get_db),
    me: User = Depends(get_current_user),
):
    if me.id == user_id:
        raise HTTPException(status_code=400, detail="cannot follow yourself")

    user = await db.get(User, user_id)
    if not user:
        raise HTTPException(status_code=404, detail="user not found")

    i_follow = bool(
        await db.scalar(
            text(
                "SELECT 1 FROM follows "
                "WHERE follower_id = :me AND followee_id = :followee_id LIMIT 1"
            ),
            {"me": me.id, "followee_id": user_id},
        )
    )
    if i_follow:
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT, detail="already following"
        )

    await db.execute(
        text(
            "INSERT INTO follows (follower_id, followee_id) "
            "VALUES (:me, :followee_id)"
        ),
        {"me": me.id, "followee_id": user_id},
    )
    await db.commit()


@app.delete("/users/{user_id}/follow", status_code=204)
async def unfollow(
    user_id: int,
    db: AsyncSession = Depends(get_db),
    me: User = Depends(get_current_user),
):
    if me.id == user_id:
        raise HTTPException(status_code=400, detail="cannot unfollow yourself")

    user = await db.get(User, user_id)
    if not user:
        raise HTTPException(status_code=404, detail="user not found")

    i_follow = bool(
        await db.scalar(
            text(
                "SELECT 1 FROM follows "
                "WHERE follower_id = :me AND followee_id = :followee_id LIMIT 1"
            ),
            {"me": me.id, "followee_id": user_id},
        )
    )
    if not i_follow:
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT, detail="not following"
        )

    await db.execute(
        text(
            "DELETE FROM follows "
            "WHERE follower_id = :me AND followee_id = :followee_id"
        ),
        {"me": me.id, "followee_id": user_id},
    )
    await db.commit()


@app.get("/users/{user_id}/followers")
async def get_followers(
    user_id: int,
    db: AsyncSession = Depends(get_db),
    me: User = Depends(get_current_user),
):
    user = await db.get(User, user_id)
    if not user:
        raise HTTPException(status_code=404, detail="user not found")

    result = await db.execute(
        select(User)
        .join(Follow, Follow.follower_id == User.id)
        .where(Follow.followee_id == user_id)
        .order_by(User.id.desc())
    )
    users = result.scalars().all()

    return [
        UserOut(id=user.id, username=user.username, status=user.status)
        for user in users
    ]


@app.get("/users/{user_id}/following")
async def get_following(
    user_id: int,
    db: AsyncSession = Depends(get_db),
    me: User = Depends(get_current_user),
):
    user = await db.get(User, user_id)
    if not user:
        raise HTTPException(status_code=404, detail="user not found")

    result = await db.execute(
        select(User)
        .join(Follow, Follow.followee_id == User.id)
        .where(Follow.follower_id == user_id)
        .order_by(User.id.desc())
    )
    users = result.scalars().all()

    return [
        UserOut(id=user.id, username=user.username, status=user.status)
        for user in users
    ]


class PostCreateIn(BaseModel):
    contents: str


class PostOut(BaseModel):
    id: int
    user_id: int
    username: str
    contents: str
    created_at: str


@app.post("/posts", status_code=201)
async def create_post(
    payload: PostCreateIn,
    db: AsyncSession = Depends(get_db),
    me: User = Depends(get_current_user),
):
    post = Post(user_id=me.id, contents=payload.contents.strip())
    db.add(post)
    await db.commit()
    await db.refresh(post)

    return PostOut(
        id=post.id,
        user_id=post.user_id,
        username=me.username,
        contents=post.contents,
        created_at=post.created_at.isoformat(),
    )


@app.get("/users/{user_id}/posts")
async def get_users_posts(
    user_id: int,
    db: AsyncSession = Depends(get_db),
    me: User = Depends(get_current_user),
):
    result = await db.execute(
        select(Post, User.username)
        .join(User, User.id == Post.user_id)
        .where(Post.user_id == user_id)
        .order_by(desc(Post.created_at))
    )
    posts = result.all()

    return [
        PostOut(
            id=post.id,
            user_id=post.user_id,
            username=username,
            contents=post.contents,
            created_at=post.created_at.isoformat(),
        )
        for (post, username) in posts
    ]


@app.get("/feed")
async def get_feed(
    db: AsyncSession = Depends(get_db), me: User = Depends(get_current_user)
):
    result = await db.execute(
        select(Post, User.username)
        .join(User, User.id == Post.user_id)
        .join(Follow, Follow.followee_id == Post.user_id)
        .where(Follow.follower_id == me.id)
        .order_by(desc(Post.created_at))
    )

    rows = result.all()

    return [
        PostOut(
            id=p.id,
            user_id=p.user_id,
            username=username,
            contents=p.contents,
            created_at=p.created_at.isoformat(),
        )
        for (p, username) in rows
    ]
