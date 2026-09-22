<template>
  <q-page padding>
    <!-- Page Header with Breadcrumbs and Navigation Links -->
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Sessions', to: { name: 'sessions' } },
        { label: 'Management' }
      ]"
      :back-to="{ name: 'sessions' }"
    />

    <!-- Main Card Container -->
    <q-card flat bordered class="q-pa-md">
      <div class="row justify-between items-center q-mb-md">
        <div class="text-h6">Sessions Management</div>
        <q-btn 
          unelevated 
          color="primary" 
          icon="o_add" 
          label="Create Session" 
          @click="openCreateDialog" 
        />
      </div>
    </q-card>

    <!-- Create / Edit Right-Side Drawer Dialog -->
    <q-dialog v-model="dialogOpen" position="right" maximized>
      <q-card class="column full-height" style="width: 500px; max-width: 100vw; height: 500px;">
        <!-- Dialog Header Banner -->
        <q-card-section class="row items-center q-pb-none bg-primary text-white">
          <div class="text-h6">{{ editing ? 'Edit Session' : 'Create Session' }}</div>
          <q-space />
          <q-btn icon="o_close" flat round dense v-close-popup />
        </q-card-section>

        <!-- Dialog Body Form Content Container -->
        <q-card-section class="col q-pa-md scroll">
          <q-form ref="formRef" greedy @submit.prevent="submitForm">
            
            <!-- Session Name Input Field -->
            <app-text-field
              v-model="form.sessionName"
              label="Session Name"
              required
              class="q-mb-md"
              :rules="[(v) => !!v || 'Session name is required']"
            />

            <!-- Dance Style Input Field -->
            <app-text-field
              v-model="form.danceStyle"
              label="Dance Style"
              class="q-mb-md"
            />

            <!-- Timing Input Field -->
            <app-text-field
              v-model="form.timing"
              label="Timing"
              class="q-mb-md"
            />

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
import { ref, reactive } from "vue";
import { useRoute, useRouter } from "vue-router";
import { sessionApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
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

// Form Data Payload Model Definition containing session attributes
const form = reactive({ 
  sessionName: "",
  danceStyle: "",
  timing: ""
});

/**
 * Initializes state context and opens the drawer dialog in Creation mode.
 */
const openCreateDialog = () => {
  editingId.value = null;
  editing.value = false;
  form.sessionName = "";
  form.danceStyle = "";
  form.timing = "";
  dialogOpen.value = true;
};

/**
 * Hydrates target record metadata and opens the drawer dialog in Update mode.
 * @param {string|number} id - Unique primary key identifier of the target entity.
 */
const openEditDialog = async (id) => {
  if (!id) {
    notify.error("Invalid record identifier.");
    return;
  }

  editingId.value = id;
  editing.value = true;
  dialogOpen.value = true;
  
  try {
    const response = await sessionApi.get(id);
    const item = response?.data?.data || response?.data || response;
    
    if (item) {
      form.sessionName = item.sessionName || item.name || "";
      form.danceStyle = item.danceStyle || item.style || "";
      form.timing = item.timing || item.time || "";
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
      sessionName: form.sessionName,
      danceStyle: form.danceStyle,
      timing: form.timing
    };

    if (editing.value && editingId.value) {
      await sessionApi.update(editingId.value, payload);
      notify.success("Session updated successfully.");
    } else {
      await sessionApi.create(payload);
      notify.success("Session created successfully.");
    }

    dialogOpen.value = false;
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};

// Expose methods if parent components need to trigger them directly
defineExpose({
  openCreateDialog,
  openEditDialog
});
</script>