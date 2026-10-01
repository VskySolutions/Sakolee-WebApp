<template>
  <app-form-dialog
    v-model="isOpen"
    :title="isEditing ? 'Edit Session' : 'Create Session'"
    :saving="saving"
    :save-label="isEditing ? 'Save' : 'Create'"
    size="sm"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-form ref="formRef" greedy>
      <app-text-field
        v-model="form.sessionName"
        label="Session Name"
        required
        class="q-mb-md"
        :error="formErrors.sessionName.hasError"
        :error-message="formErrors.sessionName.message"
        @update:model-value="formErrors.sessionName.hasError = false"
        :rules="[
          (v) => !!v?.trim() || 'Session name is required',
          (v) =>
            !v ||
            v.trim().length <= 100 ||
            'Session name cannot exceed 100 characters'
        ]"
      />

      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch, computed } from "vue";
import { classSessionApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";

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

const form = reactive({
  sessionName: "",
  active: true
});

const formErrors = reactive({
  sessionName: {
    hasError: false,
    message: ""
  }
});

watch(() => props.modelValue, (val) => {
  isOpen.value = val;
  if (val) {
    if (props.editingId && props.initialData) {
      form.sessionName = props.initialData.sessionName || props.initialData.SessionName || props.initialData.name || props.initialData.Name || "";
      form.active = props.initialData.active ?? props.initialData.Active ?? props.initialData.isActive ?? props.initialData.is_active ?? true;
    } else {
      resetFormValues();
    }
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

//Resets the form values
const resetFormValues = () => {
  form.sessionName = "";
  form.active = true;
  formErrors.sessionName.hasError = false;
  formErrors.sessionName.message = "";
};

const resetForm = () => {
  resetFormValues();
  isOpen.value = false;
};

//Submit form
const submitForm = async ({ clearDraft } = {}) => {
  formErrors.sessionName.hasError = false;
  formErrors.sessionName.message = "";

  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: form.sessionName.trim(),
      sessionName: form.sessionName.trim(),
      active: Boolean(form.active),
      isActive: Boolean(form.active)
    };

    if (props.editingId) {
      await classSessionApi.update(props.editingId, payload);
      notify.success("Session updated successfully.");
    } else {
      await classSessionApi.create(payload);
      notify.success("Session created successfully.");
    }

    clearDraft?.();
    isOpen.value = false;
    resetFormValues();
    emit("saved");
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier || getApiErrorMessage(err)?.toLowerCase().includes('already exists')) {
      formErrors.sessionName.hasError = true;
      formErrors.sessionName.message = "A session with this name already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>