from kubernetes import client, config

config.load_kube_config()

client = client.CoreV1Api()