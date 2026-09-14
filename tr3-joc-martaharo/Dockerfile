FROM python:3.10.12-slim-bullseye
ENV PIP_NO_CACHE_DIR=1
WORKDIR /workspace

# Instal·lem dependències del sistema
RUN apt-get update && apt-get install -y libgl1 libglib2.0-0 && rm -rf /var/lib/apt/lists/*

RUN pip install --upgrade pip
RUN pip install "setuptools<81" wheel

# Hem apujat onnx a 1.16.0 i deixat onnxscript lliure per evitar el conflicte
RUN pip install "torch==2.2.2" "onnx==1.16.0" onnxscript

# Instal·lem ML-Agents
RUN pip install mlagents==1.1.0

ENTRYPOINT ["mlagents-learn"]