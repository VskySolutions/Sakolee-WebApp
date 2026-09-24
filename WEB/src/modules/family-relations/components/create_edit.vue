<template>
  <!-- Side-Drawer Dialog Component: Create / Update Form Modal -->
  <q-dialog v-model="isOpen" position="right">
    <q-card class="column" style="width: 500px; max-width: 100vw; height: 500px; max-height: 70vh;">
      
      <!-- Dialog Header Banner -->
      <q-card-section class="row items-center q-pb-none bg-primary text-white">
        <div class="text-h6">{{ isEditing ? 'Edit Family Relation' : 'Create Family Relation' }}</div>
        <q-space />
        <q-btn icon="o_close" flat round dense v-close-popup />
      </q-card-section>

      <!-- Dialog Scrollable Body Content & Reactive Form Container -->
      <q-card-section class="col q-pa-md scroll">
        <!-- Loading Spinner Overlay during fetch operations in edit mode -->
        <div v-if="loading" class="row flex-center q-pa-xl">
          <q-spinner color="primary" size="40px" />
        </div>

        <q-form v-else ref="formRef" greedy @submit.prevent="submitForm">
          <!-- Primary Entity Property: Family Relation Name -->
          <app-text-field
            v-model="form.name"
            label="Relation Name"
            required
            class="q-mb-md"
            :rules="[(v) => !!v || 'Relation name is required']"
          />
        </q-form>
      </q-card-section>

      <!-- Dialog Footer Action Triggers -->
      <q-card-actions align="right" class="q-pa-md bg-grey-2">
        <q-btn flat label="Cancel" color="grey" v-close-popup />
        <q-btn unelevated color="primary" :label="isEditing ? 'Update' : 'Save'" :loading="saving" @click="submitForm" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { api, familyRelationApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

import AppTextField from "components/common/AppTextField.vue";

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  editingId: {
    type: [String, Number],
    default: null
  }
});

const emit = defineEmits(["update:modelValue", "saved"]);
const notify = useNotify();

// Local drawer visibility synchronization
const isOpen = ref(props.modelValue);
watch(() => props.modelValue, async (val) => {
  isOpen.value = val;
  if (val) {
    if (props.editingId) {
      await fetchRecordDetails(props.editingId);
    } else {
      form.name = "";
    }
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

const loading = ref(false);
const saving = ref(false);
const formRef = ref(null);

const form = reactive({
  name: ""
});

const isEditing = computed(() => !!props.editingId);

/**
 * Fetches existing record details for editing mode.
 * @param {String|Number} id - Target record primary key identifier.
 */
const fetchRecordDetails = async (id) => {
  loading.value = true;
  try {
    // Alternatively, you can use familyRelationApi.getById(id) if available in your service definition
    const response = await api.get(`/api/admin/family-relations/${id}`);
    const item = response?.data?.data || response?.data || response;
    if (item) {
      form.name = item.name || "";
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
};

/**
 * Validates form constraints and dispatches either a POST (Create) or PUT (Update) API transaction.
 */
const submitForm = async () => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    const payload = { name: form.name };

    if (isEditing.value && props.editingId) {
      await familyRelationApi.update(props.editingId, payload);
      notify.success("Family relation updated successfully.");
      emit("saved", false);
    } else {
      await familyRelationApi.create(payload);
      notify.success("Family relation created successfully.");
      emit("saved", true);
    }

    isOpen.value = false;
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};
</script>