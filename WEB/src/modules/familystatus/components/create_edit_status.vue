<template>
  <!-- 
    ============================================================
    Create & Edit Family Status Drawer Component
    ============================================================
  -->
  <app-form-drawer
    v-model="isOpen"
    :title="isEditMode ? 'Edit Family Status' : 'Create Family Status'"
    :saving="saving"
    :save-label="isEditMode ? 'Save' : 'Create'"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-form ref="formRef" greedy>
      <!-- Family Status Name Input Field with Validation Rules -->
      <app-text-field
        v-model="form.name"
        label="Status Name"
        required
        class="q-mb-md"
        :rules="[
          (v) => !!v?.trim() || 'Status name is required',
          (v) => !v || v.trim().length <= 100 || 'Name cannot exceed 100 characters'
        ]"
      />
    </q-form>
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { 
  familyStatusApi, 
  getApiErrorMessage, 
  getApiErrorCode, 
  ApiErrorCodes 
} from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDrawer from "components/common/AppFormDrawer.vue";
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
const saving = ref(false);
const formRef = ref(null);

/*
 * Determine if the component is operating in edit mode based on editingId.
 */
const isEditMode = computed(() => !!props.editingId);

/*
 * Reactive form state for family status name.
 */
const form = reactive({
  name: ""
});

/*
 * Two-way model binding wrapper for drawer visibility control.
 */
const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

/*
 * Watch drawer visibility and load existing record data if editing.
 */
watch(
  () => props.modelValue,
  async (val) => {
    if (val && props.editingId) {
      saving.value = true;
      try {
        const statusRecord = await familyStatusApi.get(props.editingId);
        form.name = statusRecord?.name || "";
      } catch (err) {
        notify.error(getApiErrorMessage(err));
        isOpen.value = false;
      } finally {
        saving.value = false;
      }
    } else if (!val) {
      resetForm();
    }
  }
);

/*
 * Reset form fields and clear validation states.
 */
const resetForm = () => {
  form.name = "";
  formRef.value?.resetValidation?.();
};

/*
 * Handle form submission for both creation and updates with duplicate checks.
 */
const submitForm = async ({ clearDraft } = {}) => {
  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: form.name.trim()
    };

    let isNew = false;
    if (isEditMode.value) {
      await familyStatusApi.update(props.editingId, payload);
      notify.success("Family status updated successfully.");
    } else {
      await familyStatusApi.create(payload);
      notify.success("Family status created successfully.");
      isNew = true;
    }

    clearDraft?.();
    isOpen.value = false;
    resetForm();
    emit("saved", isNew);
  } catch (err) {
    // Handle duplicate name duplication validation error response from API
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A family status with this name already exists.");
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>