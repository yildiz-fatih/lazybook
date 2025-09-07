import os
from contextlib import asynccontextmanager
from fastapi import Depends, FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession
from .auth import (
    generate_access_token,
    get_current_user,
    hash_password,
    verify_password,
)
from .database import engine, get_db
from .models import Base, User

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
