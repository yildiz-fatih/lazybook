from datetime import datetime
from enum import StrEnum
from sqlalchemy import (
    DateTime,
    Enum,
    ForeignKey,
    String,
    UniqueConstraint,
    func,
)
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column


# Base class for all Sqlalchemy models
class Base(DeclarativeBase):
    pass


class AuthProviderEnum(StrEnum):
    LOCAL = "local"
    GOOGLE = "google"
    GITHUB = "github"


class User(Base):
    __tablename__ = "users"
    __table_args__ = (
        UniqueConstraint("auth_provider", "auth_provider_user_id", name="ext_user"),
    )

    id: Mapped[int] = mapped_column(primary_key=True)
    username: Mapped[str | None] = mapped_column(
        String(64), unique=True, index=True, nullable=True
    )
    password_hash: Mapped[str | None] = mapped_column(String(128), nullable=True)
    status: Mapped[str] = mapped_column(nullable=False, default="")
    auth_provider: Mapped[AuthProviderEnum] = mapped_column(
        Enum(AuthProviderEnum), nullable=False, default=AuthProviderEnum.LOCAL
    )
    auth_provider_user_id: Mapped[str | None] = mapped_column(
        String, nullable=True, default=None
    )


class Follow(Base):
    __tablename__ = "follows"

    # Composite primary key
    follower_id: Mapped[int] = mapped_column(
        ForeignKey("users.id", ondelete="CASCADE"),
        primary_key=True,
        index=True,
        nullable=False,
    )
    followee_id: Mapped[int] = mapped_column(
        ForeignKey("users.id", ondelete="CASCADE"),
        primary_key=True,
        index=True,
        nullable=False,
    )


class Post(Base):
    __tablename__ = "posts"

    id: Mapped[int] = mapped_column(primary_key=True)
    user_id: Mapped[int] = mapped_column(
        ForeignKey("users.id", ondelete="CASCADE"), index=True, nullable=False
    )
    contents: Mapped[str] = mapped_column(nullable=False)
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), nullable=False
    )  # Postgres sets timestamp on insert


class Message(Base):
    __tablename__ = "messages"
    id: Mapped[int] = mapped_column(primary_key=True)
    sender_id: Mapped[int] = mapped_column(
        ForeignKey("users.id", ondelete="CASCADE"), index=True, nullable=False
    )
    recipient_id: Mapped[int] = mapped_column(
        ForeignKey("users.id", ondelete="CASCADE"), index=True, nullable=False
    )
    contents: Mapped[str] = mapped_column(nullable=False)
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), nullable=False
    )
