<template>
  <parent-dashboard v-if="isParentPortal" />
  <dashboard-page v-else />
</template>

<script setup>
import { useIsParentPortal } from "composables/useParentPortal";
import DashboardPage from "modules/dashboard/pages/DashboardPage.vue";
import ParentDashboard from "modules/dashboard/parent/ParentDashboard.vue";

// Family-contact logins get the parent-portal dashboard; everyone else the studio dashboard.
const isParentPortal = useIsParentPortal();
</script>
