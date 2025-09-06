# lazybook

a minimalist social media clone

---

## setup & run

run the db + frontend:
```bash
docker compose up -d
```

run the backend:
```bash
cd backend

python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt

uvicorn app.main:app --reload --env-file .env
```
