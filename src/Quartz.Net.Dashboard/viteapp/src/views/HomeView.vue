<template>
  <div class="home">
    <img alt="Vite logo" src="../assets/QuartzDashboard.svg" />
    <HelloWorld msg="打造 Quartz.Net  Dashbaord" />
  </div>
</template>

<script>
// @ is an alias to /src
import HelloWorld from "@/components/HelloWorld.vue";

export default {
  name: "HomeView",
  components: {
    HelloWorld
  }
};
</script>
