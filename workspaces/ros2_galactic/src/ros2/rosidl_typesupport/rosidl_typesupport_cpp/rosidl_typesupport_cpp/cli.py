# Copyright 2021 Open Source Robotics Foundation, Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import os

from ament_index_python import get_package_share_directory
from ament_index_python import get_resources

from rosidl_cli.command.generate.extensions import GenerateCommandExtension
from rosidl_cli.command.generate.helpers import legacy_generator_arguments_file
from rosidl_cli.command.translate.api import translate

from rosidl_typesupport_cpp import generate_cpp


class GenerateCppTypesupport(GenerateCommandExtension):

    def generate(
        self,
        package_name,
        interface_files,
        include_paths,
        output_path
    ):
        package_share_path = \
            get_package_share_directory('rosidl_typesupport_cpp')

        templates_path = os.path.join(package_share_path, 'resource')

        # Normalize interface definition format to .idl
        idl_interface_files = []
        non_idl_interface_files = []
        for path in interface_files:
            if not path.endswith('.idl'):
                non_idl_interface_files.append(path)
            else:
                idl_interface_files.append(path)
        if non_idl_interface_files:
            idl_interface_files.extend(translate(
                package_name=package_name,
                interface_files=non_idl_interface_files,
                include_paths=include_paths,
                output_format='idl',
                output_path=output_path / 'tmp',
            ))

        # Generate typesupport code
        typesupport_implementations = list(
            get_resources('rosidl_typesupport_cpp'))

        with legacy_generator_arguments_file(
            package_name=package_name,
            interface_files=idl_interface_files,
            include_paths=include_paths,
            templates_path=templates_path,
            output_path=output_path
        ) as path_to_arguments_file:
            return generate_cpp(
                path_to_arguments_file,
                typesupport_implementations
            )
