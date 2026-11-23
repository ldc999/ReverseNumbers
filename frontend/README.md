# Vue 3 + Vite

This template should help get you started developing with Vue 3 in Vite. The template uses Vue 3 `<script setup>` SFCs, check out the [script setup docs](https://v3.vuejs.org/api/sfc-script-setup.html#sfc-script-setup) to learn more.

Learn more about IDE Support for Vue in the [Vue Docs Scaling up Guide](https://vuejs.org/guide/scaling-up/tooling.html#ide-support).
随机号码顺序：

筛选（排除/必选） → 生成号码时直接过滤
参数设置（或条件） → IsValid 检查每注玩家号码重叠 ≤ 至少一组条件
票号约束（或条件） → CheckTicketConstraints 每张票的重叠 ≤ 至少一组条件
三步全部通过才算生成成功，任一步不满足就重新随机。