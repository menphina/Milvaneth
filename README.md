# Milvaneth (Open Source)

Project Milvaneth, named after [Milvaneth Sacrarium in Ul'dah](https://ffxiv.gamerescape.com/wiki/Milvaneth_Sacrarium), is the Chinese version of cross-world market infomation sharing platform (just like mogboard.com). Which, since there is currently no Companion App available for Chinese players (though Live Letter CN said it's WIP months ago), Milvaneth will develop a separate market data fetching technology, and will support Companion App CN API as soon as possible once it is released.

# Languages

您可以在[这里](https://github.com/menphina/Milvaneth/blob/master/README-zh-cn.md)查阅中文版本的 README 文件。

If there is a content difference, the Chinese version of README shall prevail.

如存在内容差异，以中文版本的 README 为准。

# Status Quo

For a variety of reasons, my overall personal situation has not been able to reach the baseline of operating this project as a public service independently. Therefore, it is decided to disclose the source code of this project for reference.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files to set up a free or charged service based on the modified or unmodified version of this project.

Milvaneth's source code is released under the MIT/X11 license.

# Projects

Milvaneth.Cmd: A robust state machine to handle various suitation of the game (not started / in lobby / in world / quitting). Support starting / stopping network reader and memory reader at proper moment. Can be associated with a game process safely at any time.

Thaliak: Game memory reader for character info and inventory.

Thaliak.Network: Game network reader for market info and other information.

Thaliak.Writer: Signatures, Packet Ids, and Miscellaneous.

Milvaneth.Server: A market data CRUD implemention.

# Maintance

This project will no longer be updated when the FFXIV (CN) game update. But anyone can submit PRs and Issues for the project.

Current version of Milvaneth is ready for FFXIV (CN) 5.0x.

As it is modified before publish, the source code is not guranteed to be bug-free.

# BETTERDOs

- [ ] Website interface

- [ ] ~~ACT plugin version~~ (Most Chinese players use a custom build of ACT which has different codebase)

- [ ] ~~Companion App CN API~~ (Currently not available)

- [ ] Monitoring and alerting

- [ ] Account/retainer based listing

- [ ] ~~Support for PM and buyer/seller chat~~ (after account listing is implemented)

- [ ] "Best for company seal" and "Best for reconstruction" calculator

- [ ] Ingredient price calculator

- [ ] and so on ...
