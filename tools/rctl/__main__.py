#!/usr/bin/env python3

# ======================================================================================================================
# rctl: Reality Control Tool
#
# This tool is used to deploy Reality on Docker Compose and Kubernetes.
#
# Usage:
#   rctl [OPTIONS] COMMAND [ARGS]...
#
# Options:
#   --help             Show this message and exit.
#
# Commands:
#   compose            Reality on Compose commands.
#   kubernetes         Reality on Kubernetes commands.
#
# Subcommands:
#   compose build      Builds the container images.
#   compose up         Starts Reality on Compose mode.
#   compose down        Stops Reality on Compose mode and deletes all containers.
#
#   kubernetes build   Builds the container images.
#   kubernetes up      Starts Reality on Kubernetes mode.
#   kubernetes down    Stops Reality on Kubernetes mode and deletes all services, deployments and pods.
# ======================================================================================================================

from os import path
import typer

import platforms.compose as compose
import platforms.kubernetes as kubernetes

# Create the app.
app = typer.Typer()
app.add_typer(compose.app, name="compose", help="Reality on Compose commands.")
app.add_typer(kubernetes.app, name="kubernetes", help="Reality on Kubernetes commands.")
app.add_typer(kubernetes.app, name="k8s", help="Reality on Kubernetes commands.")

# Run the app.
if __name__ == "__main__":
    app()

# TODO: Create an unified configuration file for both Docker Compose and Kubernetes mode.
# TODO: Create an utility class that parses this configuration file and generates the Docker Compose and Kubernetes files on demand.
# TODO: Implement this newly created utility class on both Docker Compose and Kubernetes mode commands.