# Milvaneth

Project Milvaneth, named after [Milvaneth Sacrarium in Ul'dah](https://ffxiv.gamerescape.com/wiki/Milvaneth_Sacrarium), is the Chinese version of cross-world market infomation sharing platform (just like mogboard.com). Which, since there is currently no Companion App available for Chinese players (though Live Letter CN said it's WIP months ago), Milvaneth will develop a separate market data fetching technology, and will support Companion App CN API as soon as possible once it is released.

# Languages

您可以在[这里](https://github.com/menphina/Milvaneth/blob/master/README-zh-cn.md)查阅中文版本的 README 文件。

If there is a content difference, the Chinese version of README shall prevail.

如存在内容差异，以中文版本的 README 为准。

# Open source policy

Due to the possibility of abuse of related technologies, Milvaneth will adopt a semi-open source strategy:

1. Dual-use components with no currently open sourced alternatives, such as memory analyzers and some other components, will have access restrictions on source code and may apply anti-reverse engineering measures on object code. (\*)

2. Implementations of predictive or statistical algorithms, if any, will be open source if the algorithm itself has been made public.

3. Components that do not meet the above criteria, such as packet analyzers and user interface parts, will be open source.

(\*) Once an open source alternative appears, the corresponding component will be made public.

Milvaneth's source code is expected to be released under the MIT/X11 license.

# About the service

Milvaneth service will be public available and free-to-use. It may accept donations after formal operation.

Since players in CN region do not need to pay for the base game and expansion packs, and per-account cost is $0.09/hr playtime. The estimated operating cost of Milvaneth will be lower than similar services in JP/US/EU regions.

# Time frame

Test has started.

Will release on 10/15/2019

# TODOs

Goals listed here will be implemented step by step after first release:

- [ ] Website interface

- [ ] ~~ACT plugin version~~ (Most Chinese players use a custom build of ACT which has different codebase)

- [ ] ~~Companion App CN API~~ (Currently not available)

- [ ] Monitoring and alerting

- [ ] Account/retainer based listing

- [ ] ~~Support for PM and buyer/seller chat~~ (after account listing is implemented)

- [ ] "Best for company seal" and "Best for reconstruction" calculator

- [ ] Ingredient price calculator

- [ ] and so on ...
