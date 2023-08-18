A tool to inform novice programmers of problems with their code structure and educate on code refactoring fundamentals in order to avoid the development of bad habits. The artefact is to be a visual studio extension that will utilise existing systems to grade a project's code structure and advise the programmer how to make improvements to their code. The intent is that the artefact will be simple and not too complex as to adhere to it's original purpose of being a tool for introductory/beginner level programmers aiming to get into the field.

Currently the artefact is functional as an extension to Visual Studio. The artefact runs a background code metrics check on the project file when the document is saved. The artefact then processes the code metrics from it's XML format using LINQ to try and determine where there might be structural issues within the code base. Using the data acquired from the code metrics check, the artefact will then output pinpoint functions to the user where issues might soon arise/have arisen along with what category this falls under (coupling, maintainability e.t.c.). The artefact then advises the user, using sources to support independant-study, how to refactor and learn from potential mistakes. The artefact also verifies that nothing is wrong with the project if the code is relatively clean. The sources are easy to access and clear and the artefact as an extension is easy to navigate for beginners who might not be aware of how to use visual studio entirely (The window should open itself requiring little setup from the user).

### Further Features (Could haves)
 - Extra code metrics to be processed like class coupling.

### Refactoring
 - Made changes to the structure of ProcessCodeMetrics(), splitting it up into multiple functions to reduce duplicate code.
 - Updated the project to make use of the visual studio developer console (avoiding a lot of processes running in the background)
 - Removed old code and implemented comments for readability

### Known bugs
 - Saving quickly immediately after another save will crash the extension. (Needs a try/catch potentially)
 - Package initialisation must be done manually for CodeMetrics, function doesn't always run correctly

### Fixed bugs
 - The project is no longer reliant on msbuild being in the system's PATH, bypassed using developer console
 - The artefact no longer struggles to identify the project file when creating code metrics occasionally
