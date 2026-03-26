<template>
  <div class="app-layout">
    <LeftMenu class="leftMenu" />
    <Header title="Privacy Policy" class="header" />

    <div class="dashboard-content">
      <h2 class="page-title">Privacy Policy</h2>
    </div>
  </div>
</template>

<script setup>
import LeftMenu from "@/components/LeftMenu.vue";
</script>

<style scoped>
/* ── Layout ── */
.app-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  grid-template-rows: auto 1fr;
  height: 100vh;
  background-color: #f4f7f9;
  overflow: hidden;
}

.leftMenu {
  grid-column: 1;
  grid-row: 1 / span 2;
  z-index: 100;
}

.header {
  grid-column: 2;
  grid-row: 1;
  z-index: 90;
}
</style>
