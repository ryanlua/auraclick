# Contributing Guidelines

We welcome contributions from the community. If you would like to contribute, please follow the guidelines below.

If this is your first time contributing, [learn how to contribute to a project through forking](https://docs.github.com/en/get-started/exploring-projects-on-github/contributing-to-a-project).

## Configure your environment

Learn how to configure your environment to develop this project. This assumes you meet the [system requirements for Windows app development](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/system-requirements).

### Using Visual Studio

If you already have Visual Studio installed and have developer mode enabled, you may import the [`.vsconfig` installation configuration file](../.vsconfig).

1. Open the Visual Studio Installer and close Visual Studio.
1. On either the **Installed** tab or the **Available** tab, select **More** > **Import configuration** on the Visual Studio product card.
1. Locate the `.vsconfig` file from the project root, and then choose **Review details**.
1. Verify that your selections are accurate, and then choose **Modify**.

Additionally, Visual Studio will automatically detect any missing components in a open solution and will prompt you to install them if not already installed.

![Solution Explorer suggests additional components](https://learn.microsoft.com/en-us/visualstudio/install/media/vs-2019/solution-explorer-config-file.png?view=vs-2022)

For more ways to install, learn how to [import or export installation configurations](https://learn.microsoft.com/en-us/visualstudio/install/import-export-installation-configurations?view=vs-2022).

## Building the project

You can build the project with Visual Studio. Before building the project, ensure [your environment is configured correctly](#configure-your-environment).

1. In **Solution Explorer**, choose or open the solution.
1. On the menu bar, choose **Build** > **Build Solution**, or press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>B</kbd>.

To debug the project, press <kbd>F5</kbd> or choose **Debug** > **Start Debugging**.

For more information, learn how to [build and clean projects and solutions in Visual Studio](https://learn.microsoft.com/en-us/visualstudio/install/import-export-installation-configurations?view=vs-2022).

## Helpful resources

Resources to help you contribute to this project and learn more about Windows app development:

* [Start developing Windows apps](https://learn.microsoft.com/en-us/windows/apps/get-started/start-here)
* [Create your first WinUI 3 (Windows App SDK) project](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/create-your-first-winui3-app)
* [Build a Hello World app using C# and WinUI 3 / Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/how-tos/hello-world-winui3)

## Rules

* **Follow the pattern of what you already see in the code.**
* Please follow as Modern C# of a style as you can and reference the [.NET Engineering Guidelines](https://github.com/dotnet/aspnetcore/wiki/Engineering-guidelines) as much as you possibly can.
* Try to package new functionality/components into libraries that have nicely defined interfaces.
* Package new functionality into classes or refactor existing functionality into a class as you extend the code.
* When adding new classes/methods/changing existing code, add new unit tests or update the existing tests.

## License

By contributing, you agree that your contributions will be licensed under the [GNU General Public License v3.0](https://github.com/ryanlua/auraclick?tab=AGPL-3.0-1-ov-file#readme).
