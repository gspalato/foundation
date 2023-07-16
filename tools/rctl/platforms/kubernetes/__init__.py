from typer import Typer

# Create the app.
app: Typer = Typer()

# Add the subcommands.
from commands import *