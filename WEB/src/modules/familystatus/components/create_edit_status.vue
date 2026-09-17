<template>
  <q-page padding>
    <!-- Page Header with Breadcrumbs and Navigation Links -->
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Family Statuses', to: { name: 'family_statuses' } },
        { label: 'Management' }
      ]"
      :back-to="{ name: 'family_statuses' }"
    />

    <!-- Main Card Container -->
    <q-card flat bordered class="q-pa-md">
      <div class="row justify-between items-center q-mb-md">
        <div class="text-h6">Family Statuses Management</div>
        <q-btn 
          unelevated 
          color="primary" 
          icon="o_add" 
          label="Create Status" 
          @click="openCreateDialog" 
        />
      </div>
    </q-card>

    <!-- Create / Edit Right-Side Drawer Dialog -->
    <q-dialog v-model="dialogOpen" position="right" maximized>
      <q-card class="column full-height" style="width: 500px; max-width: 100vw; height: 400px;">
        <!-- Dialog Header Banner -->
        <q-card-section class="row items-center q-pb-none bg-primary text-white">
          <div class="text-h6">{{ editing ? 'Edit Family Status' : 'Create Family Status' }}</div>
          <q-space />
          <q-btn icon="o_close" flat round dense v-close-popup />
        </q-card-section>

        <!-- Dialog Body Form Content Container -->
        <q-card-section class="col q-pa-md scroll">
          <q-form ref="formRef" greedy @submit.prevent="submitForm">
            <!-- Associated Tenant Selection Dropdown -->
            <app-select
              v-model="form.tenantId"
              :options="tenantOptions"
              label="Tenant"
              required
              emit-value
              map-options
              class="q-mb-md"
              :rules="[(v) => !!v || 'Tenant is required']"
            />

            <!-- Primary Family Status Name Input Field -->
            <app-text-field
              v-model="form.name"
              label="Status Name"
              required
              class="q-mb-md"
              :rules="[(v) => !!v || 'Status name is required']"
            />

            <!-- Description Field (Commented Out for Future Extensions) -->
            <!--
            <app-text-field
              v-model="form.description"
              label="Description"
              type="textarea"
              class="q-mb-md"
              hint="Optional description for this family status"
            />
            -->
          </q-form>
        </q-card-section>

        <!-- Dialog Footer Action Triggers -->
        <q-card-actions align="right" class="q-pa-md bg-grey-2">
          <q-btn flat label="Cancel" color="grey" v-close-popup />
          <q-btn unelevated color="primary" :label="editing ? 'Update' : 'Save'" :loading="saving" @click="submitForm" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { familyStatusApi, tenantApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";

// Router and Utility Composable Declarations
const route = useRoute();
const router = useRouter();
const notify = useNotify();

// Component State and Parameter References
const dialogOpen = ref(false);
const editingId = ref(null);
const editing = ref(false);
const saving = ref(false);
const formRef = ref(null);
const loading = ref(false);

// Reactive reference for storing tenant dropdown collection options
const tenantOptions = ref([]);

// Form Data Payload Model Definition
const form = reactive({ 
  tenantId: null,
  name: "", 
  // description: "" 
});

// Lifecycle Hook: Fetch Tenant Options and Assign Default Selection on Component Mount
onMounted(async () => {
  loading.value = true;
  try {
    const tenantRes = await tenantApi.list({ limit: 100 });
    const tenantsList = tenantRes?.data || tenantRes || [];
    
    tenantOptions.value = tenantsList.map((t) => ({ 
      label: t.name, 
      value: t.tenantId || t.tenantid 
    }));

    // Automatically set the first tenant as default if options exist and form tenant is empty
    if (tenantOptions.value.length > 0 && !form.tenantId) {
      form.tenantId = tenantOptions.value[0].value;
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
});

/**
 * Initializes state context and opens the drawer dialog in Creation mode.
 * Automatically assigns the first available tenant as the default value if present.
 */
const openCreateDialog = () => {
  editingId.value = null;
  editing.value = false;
  
  // Set default tenant to the first option if available, otherwise fallback to null
  form.tenantId = tenantOptions.value.length > 0 ? tenantOptions.value[0].value : null;
  form.name = "";
  // form.description = "";
  
  dialogOpen.value = true;
};

/**
 * Hydrates target record metadata and opens the drawer dialog in Update mode.
 * @param {string|number} id - Unique primary key identifier of the target entity.
 */
const openEditDialog = async (id) => {
  editingId.value = id;
  editing.value = true;
  dialogOpen.value = true;
  
  try {
    const response = await familyStatusApi.get(id);
    const item = response?.data?.data || response?.data || response;
    
    if (item) {
      form.tenantId = item.tenantId || item.tenantid || null;
      form.name = item.name || "";
      // form.description = item.description || "";
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

/**
 * Validates form integrity constraints and dispatches a POST (Create) or PUT (Update) API transaction.
 */
const submitForm = async () => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    const payload = {
      tenantId: form.tenantId,
      name: form.name,
      // description: form.description
    };

    if (editing.value && editingId.value) {
      await familyStatusApi.update(editingId.value, payload);
      notify.success("Family status updated successfully.");
    } else {
      await familyStatusApi.create(payload);
      notify.success("Family status created successfully.");
    }

    dialogOpen.value = false;
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};
</script>