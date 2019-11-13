# Milvaneth

Milvaneth 计划，取名自[乌尔达哈的密尔瓦内斯礼拜堂](https://ffxiv.gamerescape.com/wiki/Milvaneth_Sacrarium)，是中国版本的跨服市场信息共享平台（类似国际服的 mogboard.com）。由于国服目前没有手机 App（丝瓜：在做了在做了），Milvaneth 计划会独立研发一套新的物价获取技术，并在 App 发布时尽快支持通过 App 接口获取物价信息。

# Languages

You can view English version of README [here](https://github.com/menphina/Milvaneth/blob/master/README.md).

If there is a content difference, the Chinese version of README shall prevail.

如存在内容差异，以中文版本的 README 为准。

# 现状说明

由于种种个人原因，我认为目前自己无法独自将此项目部署为一个公共免费服务。因此，我决定公开既有代码以供后来者参考。

特此免费授予获得该软件和相关文档文件副本的任何人基于该项目的修改或未修改的版本设置免费或收费服务的权利。

Milvaneth 项目的源代码以 MIT/X11 许可证分发。

# 子项目

Milvaneth.Cmd: 一个健壮的游戏状态机。支持在正确的时机启停网络和内存访问，支持在任意时机介入游戏进程。

Thaliak: 角色数据和物品信息内存读取器。

Thaliak.Network: 市场数据和其他数据网络读取器。

Thaliak.Writer: 内存签名、网络包 Id 及杂项。

Milvaneth.Server: 一个支持市场数据 CRUD 的服务器端 API 实现。

# 维护状况

本项目将不再跟随 FF14 国服客户端的版本更新进行升级，但我会响应PR和Issue。

目前本项目的数据适用于 FF14 国服的 5.0x 版本。

由于在发布前进行了一定程度的修改，本次公开的源代码不保证可依期望运行。

# 未竟事项

- [ ] 网页版物价浏览

- [ ] ~~ACT 插件版本及对应 Overlay~~（国服使用的是修改后的 ACT，需要与另一方合作）

- [ ] ~~使用手机 App 的 API~~（目前无法实现）

- [ ] 自定义物价监控和提醒

- [ ] 账户和雇员所有权登记和自定义列表

- [ ] ~~支持“销售招募版”和私信/聊天功能~~（在上一条实现后）

- [ ] “军票性价比”和“多玛重建性价比”计算器（若游戏后续版本推出其他物品交换功能，会追加相应计算器）

- [ ] 制作物品原料总价计算器和采购路线规划器

- [ ] 制作物品成本统计

- [ ] 未完待续……
