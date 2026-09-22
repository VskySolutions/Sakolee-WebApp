<template>
  <q-page padding>
    <!-- Page Header: Renders navigation breadcrumbs and route actions -->
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Family Statuses', to: { name: 'family_statuses' } },
        { label: 'View Family Status' }
      ]"
      :back-to="{ name: 'family_statuses' }"
    />

    <!-- Loading Spinner Overlay: Displayed during asynchronous data retrieval -->
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <!-- Main Card Container: Wraps the entity details view interface -->
    <q-card v-else flat bordered class="q-pa-md">
      <q-form class="q-gutter-md">
        
        <!-- Primary Entity Field: Family status name input (Disabled for view-only) -->
        <app-text-field
          v-model="form.name"
          label="Status Name"
          readonly
          disable
          class="q-mb-md"
        />

        <!-- Form Action Buttons: Back/Close navigation trigger -->
        <div class="row q-gutter-sm justify-end">
          <q-btn unelevated color="primary" label="Back" :to="{ name: 'family_statuses' }" />
        </div>
      </q-form>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import { useRoute } from "vue-router";
import { api, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
import AppTextField from "components/common/AppTextField.vue";

// Composables & Instance Initializations
const route = useRoute();
const notify = useNotify();

// Component State & Parameter References
const familyStatusId = route.params.id;
const loading = ref(false);

// Form Data Model Binding Structure (View-only, containing only status name)
const form = reactive({ 
  name: "", 
});

// Global Error Handler
const onError = (err) => {
  notify.error(getApiErrorMessage(err));
};

// Lifecycle Hooks & Data Fetching
onMounted(async () => {
  loading.value = true;

  try {
    // Fetch existing record details to display
    if (familyStatusId) {
      const response = await api.get(`/api/admin/family-statuses/${familyStatusId}`);
      const item = response?.data?.data || response?.data;
      
      if (item) {
        form.name = item.name || "";
      }
    }
  } catch (err) {
    onError(err);
  } finally {
    loading.value = false;
  }
});
</script>