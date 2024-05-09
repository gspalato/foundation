#!/usr/bin/env python3

# ======================================================================================================================
# fctl: Foundation Control Tool
#
# This tool is used to deploy Foundation on Docker Compose and Kubernetes.
#
# Usage:
#   fctl [OPTIONS] COMMAND [ARGS]...
#
# Options:
#   --help             Show this message and exit.
#
# Commands:
#   compose            Foundation on Compose commands.
#   kubernetes         Foundation on Kubernetes commands.
#
# Subcommands:
#   compose build      Builds the container images.
#   compose up         Starts Foundation on Compose mode.
#   compose down        Stops Foundation on Compose mode and deletes all containers.
#
#   kubernetes build   Builds the container images.
#   kubernetes up      Starts Foundation on Kubernetes mode.
#   kubernetes down    Stops Foundation on Kubernetes mode and deletes all services, deployments and pods.
# ======================================================================================================================

from os import path
import typer

import platforms.compose as compose
import platforms.kubernetes as kubernetes

# Create the app.
app = typer.Typer()
app.add_typer(compose.app, name="compose", help="Foundation on Compose commands.")
app.add_typer(kubernetes.app, name="kubernetes", help="Foundation on Kubernetes commands.")
app.add_typer(kubernetes.app, name="k8s", help="Foundation on Kubernetes commands.")

# Run the app.
if __name__ == "__main__":
    app()

# TODO: Create an unified configuration file for both Docker Compose and Kubernetes mode.
# TODO: Create an utility class that parses this configuration file and generates the Docker Compose and Kubernetes files on demand.
# TODO: Implement this newly created utility class on both Docker Compose and Kubernetes mode commands.