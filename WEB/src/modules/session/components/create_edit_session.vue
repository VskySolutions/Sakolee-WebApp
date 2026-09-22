<template>
  <q-page padding>
    <!-- Page Header with Breadcrumbs and Navigation Links -->
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Sessions', to: { name: 'sessions' } },
        { label: editing ? 'Edit Session' : 'Create Session' }
      ]"
      :back-to="{ name: 'sessions' }"
    />

    <!-- Create / Edit Right-Side Drawer Dialog (Directly Opens) -->
    <q-dialog v-model="dialogOpen" position="right" maximized persistent>
      <q-card class="column" style="width: 500px; max-width: 100vw; height: 500px; max-height: 70vh;">
        <!-- Dialog Header Banner -->
        <q-card-section class="row items-center q-pb-none bg-primary text-white">
          <div class="text-h6">{{ editing ? 'Edit Session' : 'Create Session' }}</div>
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
            <!-- Session Name Input Field -->
            <app-text-field
              v-model="form.sessionName"
              label="Session Name"
              required
              class="q-mb-md"
              :rules="[(v) => !!v || 'Session name is required']"
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
import { classSessionApi, getApiErrorMessage } from "services/api";
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

// Form Data Payload Model Definition
const form = reactive({ 
  sessionName: ""
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
      const response = await classSessionApi.get(id);
      const item = response?.data?.data || response?.data || response;
      
      if (item) {
        form.sessionName = item.sessionName || item.SessionName || item.name || "";
      }
    } catch (err) {
      notify.error(getApiErrorMessage(err));
    } finally {
      loading.value = false;
    }
  }
});

/**
 * Closes the popup and redirects back to the sessions list.
 */
const closeDialog = () => {
  dialogOpen.value = false;
  router.push({ name: 'sessions' });
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
      Name: form.sessionName,
      sessionName: form.sessionName,
      active: true
    };

    if (editing.value && editingId.value) {
      await classSessionApi.update(editingId.value, payload);
      notify.success("Session updated successfully.");
    } else {
      await classSessionApi.create(payload);
      notify.success("Session created successfully.");
    }

    dialogOpen.value = false;
    router.push({ name: 'sessions' });
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};
</script>