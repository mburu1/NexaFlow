# Kubernetes manifests (scaffold)

Structurally correct manifests for the NexaFlow stack, mirroring `docker-compose.yml`.
**Not yet applied or validated against a live cluster** — Phase 4 work. Use the
[Helm chart](../helm/nexaflow) instead for anything beyond a quick read.

## Apply order

```bash
kubectl apply -f namespace.yaml
kubectl apply -f configmap.yaml
kubectl apply -f secret.example.yaml   # copy, fill in real values, rename first
kubectl apply -f redis-deployment.yaml
kubectl apply -f rabbitmq-deployment.yaml
kubectl apply -f kafka-deployment.yaml
kubectl apply -f api-deployment.yaml
kubectl apply -f api-service.yaml
```

`secret.example.yaml` is a template, not a real Secret — never commit filled-in credentials.
