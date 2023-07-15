from component import Component
import os
import yaml
from yaml.loader import SafeLoader

class Configuration:
    def __init__(self):
        self.api = None
        self.components = []

    def load_from_file(self, path: str):
        if (not os.path.isfile(path)):
            raise FileNotFoundError("No services.yml file found in " + path + ".")

        with open(path) as f:
            data = yaml.load(f, Loader=SafeLoader)

            if (not data["api"] or data["api"] == ""):
                raise ValueError("Invalid API name.")
            
            # Load other data.
            self.api = data["api"]

            components = []

            # Append components to respective lists.
            for entry in data["components"]:
                if (entry["type"] not in ["database", "microservice", "application"]):
                    raise ValueError("Invalid component type '" + entry["type"] + "'.")

                _id = data["api"] + "." + entry["type"] + "." + entry["name"]
                name = entry["name"]
                _type = entry["type"]
                path = entry["path"]

                component = Component(_id, name, _type, path)
                components.append(component)
            
            # Append all components to the ordered all list.
            for entry in data["order"]:
                component = [component for (_, component) in enumerate(components) if component.name == entry["name"] and component.type == entry["type"]]
                if (not component):
                    continue
                self.components.append(component[0])


                