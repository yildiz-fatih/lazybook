import random
from coolname import generate_slug
from sqlalchemy import select

from .models import User

MAX_USERNAME_LEN = 32


# generates a unique username that's not taken in the DB
async def generate_unique_username(db) -> str:
    for _ in range(50):
        parts = generate_slug().split("-")[:2]
        base = "".join(word.capitalize() for word in parts)
        username = f"{base}{random.randint(1, 99)}"

        if len(username) > MAX_USERNAME_LEN:
            continue

        # check DB if this username already exists
        result = await db.execute(select(User).where(User.username == username))
        existing = result.scalars().first()
        if not existing:
            return username

    raise RuntimeError("Could not generate unique username")
