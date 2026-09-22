<template>
  <div class="aside-header flex items-center no-wrap" :class="menuCollapsed ? 'pa-10' : 'pa-24'">
    <!-- <img
      src="~assets/logo.png"
      width="40"
      height="40"
      alt="Sakolee"
      class="logo w-10 h-10 rounded-xl bg-gradient-to-tr from-primary to-indigo-500 flex items-center justify-center text-white shadow-lg shadow-primary/20"
    > -->
    <div class="w-10 h-10 rounded-xl bg-gradient-to-tr from-primary to-indigo-500 flex items-center justify-center text-white shadow-lg shadow-primary/20">
      <span class="material-symbols-outlined fs-26" style="font-variation-settings: &quot;FILL&quot; 1;">school</span>
    </div>
    <div class="fw-700 fs-22 text-2e q-mini-drawer-hide">
      Sakolee
    </div>
    <div class="fw-700 fs-22 text-2e" :class="$q.screen.width < 1024 ? '' : 'hidden'">
      Sakolee
    </div>
  </div>
</template>
<style scoped lang="scss">
.aside-header {
  height: 88px;
  gap: 12px;
}
.pa-24{ padding: 24px !important; }
.pa-10{ padding: 10px !important; }
</style>
