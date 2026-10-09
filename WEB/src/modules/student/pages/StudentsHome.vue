<template>
  <parent-students v-if="isParentPortal" />
  <students-list v-else />
</template>

<script setup>
import { useIsParentPortal } from "composables/useParentPortal";
import StudentsList from "modules/student/pages/index.vue";
import ParentStudents from "modules/student/parent/ParentStudents.vue";

// Family-contact logins get the parent-portal "My Students" cards; everyone else the students table.
const isParentPortal = useIsParentPortal();
</script>
