# RaccoltaASM - Docker

## Build dell'immagine

```bash
docker build -t fratellobigio/raccolta-asm:latest .
```

Se vuoi indicare anche la versione in locale:

```bash
docker build --build-arg VERSION=1.2.3 -t fratellobigio/raccolta-asm:1.2.3 .
```

Se vuoi renderlo parametrico in locale, sostituisci `fratellobigio` con il tuo username Docker Hub.

## Esecuzione del container

```bash
docker run -d -p 8080:8080 --name raccolta fratellobigio/raccolta-asm:latest
```

L'API sarà disponibile su `http://localhost:8080`

## Endpoints

- **GET /** - Informazioni sull'API
- **GET /raccolta** - Calendario della raccolta (con parametri `inizio` e `fine`)
- **GET /health** - Health check

## Esempi di richiesta

```bash
curl http://localhost:8080/
```

```bash
curl "http://localhost:8080/raccolta?inizio=2026-05-09&fine=2026-05-16"
```

## Port mapping

Il container espone la porta **8080** di default. Per cambiarla:

```bash
docker run -d -p 9000:8080 --name raccolta fratellobigio/raccolta-asm:latest
```

Sarà disponibile su `http://localhost:9000`

## Arresto del container

```bash
docker stop raccolta
```
