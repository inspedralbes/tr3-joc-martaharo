FROM python:3.10.12-slim-bullseye
WORKDIR /workspace
RUN apt-get update && apt-get install -y libgl1 libglib2.0-0 && rm -rf /var/lib/apt/lists/*
RUN pip install --upgrade pip
RUN pip install --no-cache-dir mlagents==1.1.0
ENTRYPOINT ["mlagents-learn"]
