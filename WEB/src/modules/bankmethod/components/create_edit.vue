<template>
  <q-page padding>
    <!-- Page Header with Breadcrumbs and Navigation Links -->
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Billing Methods', to: { name: 'bank_methods' } },
        { label: editing ? 'Edit Billing Method' : 'Create Billing Method' }
      ]"
      :back-to="{ name: 'bank_methods' }"
    />

    <!-- Create / Edit Right-Side Drawer Dialog (Directly Opens) -->
    <q-dialog v-model="dialogOpen" position="right" maximized persistent>
      <q-card class="column" style="width: 500px; max-width: 100vw; height: 500px; max-height: 70vh;">
        <!-- Dialog Header Banner -->
        <q-card-section class="row items-center q-pb-none bg-primary text-white">
          <div class="text-h6">{{ editing ? 'Edit Billing Method' : 'Create Billing Method' }}</div>
          <q-space />
          <q-btn icon="o_close" flat round dense @click="closeDialog" />
        </q-card-section>

        <!-- Dialog Body Form Content Container -->
        <q-card-section class="col q-pa-md scroll">
          <!-- Loading Spinner Overlay during fetch -->
          <div v-if="loading" class="row flex-center q-pa-xl">
            <q-spinner color="primary" size="40px" />
          </div>

          <q-form v-else ref="formRef" greedy @submit.prevent="submitForm">
            <!-- Billing  Method Name Input Field -->
            <app-text-field
              v-model="form.name"
              label="Billing Method Name"
              required
              :error="!!nameDuplicateError"
              :error-message="nameDuplicateError"
              @update:model-value="nameDuplicateError = ''"
              class="q-mb-md"
              :rules="[(v) => !!v || 'Billing method name is required']"
            />

            <!-- Active Status Toggle Field -->
            <q-toggle
              v-model="form.active"
              label="Active"
              class="q-mb-md"
            />
          </q-form>
        </q-card-section>

        <!-- Dialog Footer Action Triggers -->
        <q-card-actions align="right" class="q-pa-md bg-grey-2">
          <q-btn flat label="Cancel" color="grey" @click="closeDialog" />
          <q-btn unelevated color="primary" :label="editing ? 'Update' : 'Save'" :loading="saving" @click="submitForm" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { billingMethodApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
import AppTextField from "components/common/AppTextField.vue";

// Router and Utility Composable Declarations
const route = useRoute();
const router = useRouter();
const notify = useNotify();

// Component State References
const dialogOpen = ref(true); // Automatically open on page load
const editingId = ref(null);
const editing = ref(false);
const loading = ref(false);
const saving = ref(false);
const formRef = ref(null);

const nameDuplicateError = ref(""); 

// Form Data Payload Model Definition
const form = reactive({ 
  name: "",
  active: true
});

/**
 * Component Lifecycle Hook: Directly opens the dialog on load and checks for edit mode.
 */
onMounted(async () => {
  const id = route.query.id;
  if (id) {
    editing.value = true;
    editingId.value = id;
    loading.value = true;
    
    try {
      const response = await billingMethodApi.get(id);
      const item = response?.data?.data || response?.data || response;
      
      if (item) {
        form.name = item.name || item.Name || "";
        form.active = item.active !== undefined ? item.active : (item.Active !== undefined ? item.Active : true);
      }
    } catch (err) {
      notify.error(getApiErrorMessage(err));
    } finally {
      loading.value = false;
    }
  }
});

/**
 * Closes the popup and redirects back to the bank methods list.
 */
const closeDialog = () => {
  dialogOpen.value = false;
  router.push({ name: 'bank_methods' });
};

/**
 * Validates form integrity constraints and dispatches a POST (Create) or PUT (Update) API transaction.
 */
const submitForm = async () => {

  nameDuplicateError.value = "";
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    const payload = {
      name: form.name,
      active: form.active
    };

    if (editing.value && editingId.value) {
      await billingMethodApi.update(editingId.value, payload);
      notify.success("Billing method updated successfully.");
    } else {
      await billingMethodApi.create(payload);
      notify.success("Billing method created successfully.");
    }

    dialogOpen.value = false;
    router.push({ name: 'bank_methods' });
  } catch (err) {
    
    
    notify.error(getApiErrorMessage(err));

    // const status = err?.response?.status || err?.status;
    // if (status === 409) {
    //   nameDuplicateError.value = "A billing method with this name already exists.";
    // } else {
    //   notify.error(getApiErrorMessage(err));
    // }
  } finally {
    saving.value = false;
  }
};
</script>