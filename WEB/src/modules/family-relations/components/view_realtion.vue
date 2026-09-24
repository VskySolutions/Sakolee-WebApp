<template>
  <q-page padding>
    <!-- Page Header Component: Manages navigation breadcrumbs and back route actions -->
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Family Relations', to: { name: 'family_relations' } },
        { label: 'View Family Relation' }
      ]"
      :back-to="{ name: 'family_relations' }"
    />

    <!-- Loading Spinner Overlay: Displayed during asynchronous data retrieval operations -->
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <!-- Main Card Container: Wraps the entity details view interface -->
    <q-card v-else flat bordered class="q-pa-md">
      <q-form class="q-gutter-md">
        
        <!-- Family Relation Name Field (View-Only) -->
        <app-text-field
          v-model="form.name"
          label="Family Relation Name"
          readonly
          disable
          class="q-mb-md"
        />

        <!-- Active Status Field (View-Only with text formatting) -->
        <app-text-field
          v-model="form.active"
          label="Active Status"
          readonly
          disable
          class="q-mb-md"
        />

        <!-- Tenant Name Field (View-Only with fallback handling) -->
        <app-text-field
          v-model="form.tenantName"
          label="Tenant Name"
          readonly
          disable
          class="q-mb-md"
        />

        <!-- Created Date Field (View-Only) -->
        <app-text-field
          v-model="form.createdOn"
          label="Created Date"
          readonly
          disable
          class="q-mb-md"
        />

        <!-- Form Action Buttons: Back navigation trigger -->
        <div class="row q-gutter-sm justify-end">
          <q-btn unelevated color="primary" label="Back" @click="$router.push({ name: 'family_relations' })" />
        </div>
      </q-form>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { familyRelationApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useTenantOptions } from "composables/useTenantOptions";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
import AppTextField from "components/common/AppTextField.vue";

// Composables & Route Initializations
const route = useRoute();
const router = useRouter();
const notify = useNotify();
const { tenantOptions, loadTenants } = useTenantOptions();

// Component State & Parameter References
const relationId = route.params.id;
const loading = ref(false);

// Form Data Model Binding Structure for View Details
const form = reactive({ 
  name: "", 
  active: "",
  tenantName: "",
  createdOn: ""
});

/**
 * Handles global error notificationing.
 * @param {Object} err - Error object returned from API call.
 */
const onError = (err) => {
  notify.error(getApiErrorMessage(err));
};

// Lifecycle Hook: Fetches record details and tenant information on component mount
onMounted(async () => {
  if (!relationId) {
    notify.error("Invalid family relation identifier provided in route.");
    return;
  }

  loading.value = true;

  try {
    // Attempt loading tenant options safely
    try {
      if (loadTenants) await loadTenants();
    } catch (tenantErr) {
      console.warn("Could not load tenant options:", tenantErr);
    }

    // Fetch existing record details from API endpoint
    const response = await familyRelationApi.get(relationId);
    const item = response?.data?.data || response?.data || response;
    
    if (item) {
      form.name = item.name || item.Name || item.familyRelationName || item.FamilyRelationName || "";
      
      // Format active status boolean to descriptive string
      const isActive = item.active !== undefined ? item.active : item.Active;
      form.active = isActive ? 'Yes' : 'No';
      
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
      
      // Format creation date string if available
      const rawDate = item.createdOnUtc || item.CreatedOnUtc || item.createdOn || item.CreatedOn || item.createdAt || item.CreatedAt;
      if (rawDate) {
        const date = new Date(rawDate);
        form.createdOn = isNaN(date.getTime()) ? String(rawDate) : date.toLocaleString();
      } else {
        form.createdOn = "-";
      }
    } else {
      notify.error("Family relation record details could not be found.");
    }
  } catch (err) {
    console.error("Error fetching family relation details:", err);
    onError(err);
  } finally {
    loading.value = false;
  }
});
</script>