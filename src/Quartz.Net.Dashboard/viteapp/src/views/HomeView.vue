<template>
  <div class="home">
    <img alt="Vite logo" src="../assets/vue.svg" />
    <HelloWorld msg="Welcome to Your Vue|Vite App" />
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
