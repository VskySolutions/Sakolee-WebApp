<template>
  <!-- Form Dialog for Creating or Editing Family Status -->
  <app-form-dialog
    v-model="isOpen"
    :title="isEditing ? 'Edit Family Status' : 'Create Family Status'"
    :saving="saving"
    :save-label="isEditing ? 'Save' : 'Create'"
    size="sm"
    @submit="submitForm"
    @cancel="resetForm"
  >
  <!-- Form for Family Status -->
    <q-form ref="formRef" greedy>
      <app-text-field
        v-model="form.familyStatusName"
        label="Family Status Name"
        required
        class="q-mb-md"
        :error="formErrors.familyStatusName.hasError"
        :error-message="formErrors.familyStatusName.message"
        @update:model-value="formErrors.familyStatusName.hasError = false"
        :rules="[
          (v) => !!v?.trim() || 'Family status name is required',
          (v) =>
            !v ||
            v.trim().length <= 100 ||
            'Family status name cannot exceed 100 characters'
        ]"
      />

      <!-- Toggle for Active Status -->
      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-dialog>
</template>

<script setup>

import { ref, reactive, watch, computed } from "vue";
import { familyStatusApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";

//Props And Emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editingId: { type: [Object, String, Number], default: null },
  initialData: { type: Object, default: null }
});

const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const saving = ref(false);
const formRef = ref(null);
const isOpen = ref(props.modelValue);

const isEditing = computed(() => !!props.editingId);

// Reactive form data and error tracking
const form = reactive({
  familyStatusName: "",
  active: true
});

// Reactive form error tracking
const formErrors = reactive({
  familyStatusName: {
    hasError: false,
    message: ""
  }
});

// Watch for changes in modelValue prop to open/close the dialog and populate form data
watch(() => props.modelValue, (val) => {
  isOpen.value = val;
  if (val) {
    if (props.editingId && props.initialData) {
      form.familyStatusName = props.initialData.familyStatusName || props.initialData.FamilyStatusName || props.initialData.name || props.initialData.Name || "";
      form.active = props.initialData.active ?? props.initialData.Active ?? props.initialData.is_active ?? true;
    } else {
      resetFormValues();
    }
  }
});

// Watch for changes in isOpen to emit updates to the parent component
watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

//Reset Form Values
const resetFormValues = () => {
  form.familyStatusName = "";
  form.active = true;
  formErrors.familyStatusName.hasError = false;
  formErrors.familyStatusName.message = "";
};

const resetForm = () => {
  resetFormValues();
  isOpen.value = false;
};

//Submit the form
const submitForm = async ({ clearDraft } = {}) => {
  formErrors.familyStatusName.hasError = false;
  formErrors.familyStatusName.message = "";

  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: form.familyStatusName.trim(),
      familyStatusName: form.familyStatusName.trim(),
      active: form.active
    };

    if (props.editingId) {
      await familyStatusApi.update(props.editingId, payload);
      notify.success("Family status updated successfully.");
    } else {
      await familyStatusApi.create(payload);
      notify.success("Family status created successfully.");
    }

    clearDraft?.();
    isOpen.value = false;
    resetFormValues();
    emit("saved");
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier || getApiErrorMessage(err)?.toLowerCase().includes('already exists')) {
      formErrors.familyStatusName.hasError = true;
      formErrors.familyStatusName.message = "A family status with this name already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>