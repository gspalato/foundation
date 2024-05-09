from typing import List
import os
import yaml
from yaml.loader import SafeLoader

from lib.component import Component, ComponentBuildSettings, ComponentPlatformBuildSettings
from lib.directories import ROOT_DIR

class Configuration:
    def __init__(self):
        self.api: str = ""
        self.settings = {
            "kubectl_command": ["kubectl"],
            "registry": None,
            "secrets_file": "./secrets.yml"
        }
        self.components: List[Component] = []

    def load_from_file(self, path: str):
        if (not os.path.isfile(path)):
            raise FileNotFoundError("No foundation.yml file found in " + path + ".")

        with open(path) as f:
            data = yaml.load(f, Loader=SafeLoader)

            if (not data["api"] or data["api"] == ""):
                raise ValueError("Invalid API name.")
            
            # Load other data.
            self.api = data["api"]

            # Load settings.
            settings = data["settings"]
            kubectl_command = settings["kubectl_command"]
            registry = settings["registry"]
            secrets_file = settings["secrets_file"]

            self.settings["kubectl_command"] = kubectl_command
            self.settings["registry"] = registry
            self.settings["secrets_file"] = secrets_file

            components = []

            # Append components to respective lists.
            for entry in data["components"]:
                if (entry["type"] not in ["database", "microservice", "application"]):
                    raise ValueError(f"Invalid component type '{entry['type']}'.")

                _id = f"{data['api']}-{entry['type']}-{entry['name']}"
                name = entry["name"]
                _type = entry["type"]
                path = entry["path"]

                context = entry["build"]["context"]
                dockerfile = entry["build"]["dockerfile"]
                build_on_compose = entry["build"]["platforms"]["compose"]["build"]
                push_on_compose = entry["build"]["platforms"]["compose"]["push"]
                build_on_kubernetes = entry["build"]["platforms"]["kubernetes"]["build"]
                push_on_kubernetes = entry["build"]["platforms"]["kubernetes"]["push"]

                platform_build_settings = ComponentPlatformBuildSettings(build_on_compose, build_on_kubernetes,
                                                                         push_on_compose, push_on_kubernetes)
                
                build_settings = ComponentBuildSettings(context, dockerfile, platform_build_settings)

                component = Component(_id, name, _type, path, build_settings)

                components.append(component)
            
            # Append all components to the ordered all list.
            for entry in data["order"]:
                component = [component for (_, component) in enumerate(components) if component.name == entry["name"] and component.type == entry["type"]]
                if (not component):
                    continue
                self.components.append(component[0])

configuration = Configuration()
configuration.load_from_file(os.path.join(ROOT_DIR, 'foundation.yml'))