<template>
  <h1>{{ msg }}</h1>
  <br/>
  <p class="h3">
    <a href="https://www.dotblogs.com.tw/nethawk/2024/10/02/quartz-net-dashboard" target="_blank">以 .Net Core & SingalR 打造 Quartz.Net Dashboard</a>
    <br />
    <a href="https://github.com/nethawkChen/Quartz.Net-Dashboard" target="_blank">GitHub</a>
  </p>
</template>

<script setup>
import { ref } from 'vue'

defineProps({
  msg: String,
})

</script>

<style scoped>
.read-the-docs {
  color: #888;
}
</style>
