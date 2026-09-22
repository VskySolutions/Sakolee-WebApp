<template>
  <q-page padding>
    <!-- Page Header: Renders navigation breadcrumbs and route actions -->
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Sessions', to: { name: 'sessions' } },
        { label: 'View Session' }
      ]"
      :back-to="{ name: 'sessions' }"
    />

    <!-- Loading Spinner Overlay: Displayed during asynchronous data retrieval -->
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <!-- Main Card Container: Wraps the entity details view interface -->
    <q-card v-else flat bordered class="q-pa-md">
      <q-form class="q-gutter-md">
        
        <!-- Session Name Field (View-only) -->
        <app-text-field
          v-model="form.sessionName"
          label="Session Name"
          readonly
          disable
          class="q-mb-md"
        />

        <!-- Tenant Name Field (View-only with fallback handling) -->
        <app-text-field
          v-model="form.tenantName"
          label="Tenant Name"
          readonly
          disable
          class="q-mb-md"
        />


        <!-- Created Date Field (View-only) -->
        <app-text-field
          v-model="form.createdOn"
          label="Created Date"
          readonly
          disable
          class="q-mb-md"
        />

        <!-- Form Action Buttons: Back/Close navigation trigger -->
        <div class="row q-gutter-sm justify-end">
          <q-btn unelevated color="primary" label="Back" :to="{ name: 'sessions' }" />
        </div>
      </q-form>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import { useRoute } from "vue-router";
import { classSessionApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useTenantOptions } from "composables/useTenantOptions";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
import AppTextField from "components/common/AppTextField.vue";

// Composables & Instance Initializations
const route = useRoute();
const notify = useNotify();
const { tenantOptions, loadTenants } = useTenantOptions();

// Component State & Parameter References
const sessionId = route.params.id;
const loading = ref(false);

// Form Data Model Binding Structure for View Details
const form = reactive({ 
  sessionName: "", 
  tenantName: "",
  danceStyle: "",
  timing: "",
  createdOn: ""
});

// Global Error Handler
const onError = (err) => {
  notify.error(getApiErrorMessage(err));
};

// Lifecycle Hooks & Data Fetching
onMounted(async () => {
  if (!sessionId) {
    notify.error("Invalid session identifier provided in route.");
    return;
  }

  loading.value = true;

  try {
    // Try loading tenants safely without breaking the main flow if it fails
    try {
      if (loadTenants) await loadTenants();
    } catch (tenantErr) {
      console.warn("Could not load tenant options:", tenantErr);
    }

    // Fetch existing record details to display
    const response = await classSessionApi.get(sessionId);
    console.log("API Response for Session Details:", response); // Debugging ke liye

    // Extract item payload safely based on standard API wrappers
    const item = response?.data?.data || response?.data || response;
    
    if (item) {
      form.sessionName = item.sessionName || item.SessionName || item.name || "";
      
      // Resolve tenant name with multiple fallbacks
      let resolvedTenant = item.tenantName || item.tenant_name || item.tenant?.name;
      if (!resolvedTenant && tenantOptions?.value) {
        const tenantId = item.tenantId || item.tenantid || item.TenantId;
        if (tenantId) {
          const found = tenantOptions.value.find((t) => t.value === tenantId || t.id === tenantId);
          resolvedTenant = found ? found.label : tenantId;
        }
      }
      form.tenantName = resolvedTenant || "-";

      form.danceStyle = item.danceStyle || item.DanceStyle || item.style || "";
      form.timing = item.timing || item.Timing || item.time || "";
      
      // Format creation date string if available
      const rawDate = item.createdOn || item.CreatedOn || item.createdAt || item.CreatedAt;
      if (rawDate) {
        const date = new Date(rawDate);
        form.createdOn = isNaN(date.getTime()) ? String(rawDate) : date.toLocaleString();
      } else {
        form.createdOn = "-";
      }
    } else {
      notify.error("Session record details could not be found.");
    }
  } catch (err) {
    console.error("Error fetching session details:", err);
    onError(err);
  } finally {
    loading.value = false;
  }
});
</script>