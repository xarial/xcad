<template>
  <div class="video_wrapper">
    <iframe
      frameborder="0"
      width="100%"
      height="100%"
      :src="'https://www.youtube.com/embed/' + props.id"
      allowfullscreen
    ></iframe>
  </div>
</template>
<script setup>
const props = defineProps(["id"]);
</script>
<style scoped>
.video_wrapper {
  position: relative;
  padding-bottom: 56.25%;
  /* 16:9, for an aspect ratio of 1:1 change to this value to 100% */
}

iframe {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
}
</style>
