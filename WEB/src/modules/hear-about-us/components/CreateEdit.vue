<template>
  <!-- Drawer used for creating and editing a Hear About Us record -->
  <app-form-dialog
    v-model="formOpen"
    :title="editing ? 'Edit Hear About Us' : 'Create Hear About Us'"
    :saving="saving"
    :save-label="editing ? 'Save' : 'Create'"
    size="sm"
    @submit="submit"
    @cancel="reset"
  >
    <!-- Form container with validation -->
    <q-form ref="formRef" greedy>
      <!-- Hear About Us name field -->
      <app-text-field
        v-model="form.name"
        label="Name"
        required
        class="q-mb-md"
        :error="!!nameError"
        :error-message="nameError"
        :rules="[
          (v) => !!v?.trim() || 'Hear About Us name is required'
        ]"
        @update:model-value="clearNameError"
      />
      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-dialog>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { hearAboutUsApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";
// Define the properties received from the parent component.
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  // Indicates whether the form is used for editing.
  editing: { type: Boolean, default: false },
  // Contains the Hear About Us data when editing.
  hearAboutUs: { type: Object, default: null }
});
// Events sent back to the parent component.
const emit = defineEmits(["update:modelValue", "saved"]);
const notify = useNotify();
const formOpen = ref(props.modelValue);
const formRef = ref(null);
const saving = ref(false);
const nameError = ref("");
// Form data.
const form = reactive({ name: "", active: true });
// Watch for changes to the drawer state from the parent.
watch(() => props.modelValue, (value) => {
  formOpen.value = value;
  // Load form data when the drawer is opened.
  if (value) { loadForm(); }
});
// Send drawer state changes back to the parent.
watch(formOpen, (value) => {
  emit("update:modelValue", value);
});
// Load existing Hear About Us data when editing.
const loadForm = () => {
  form.name = props.hearAboutUs?.name || "";
  form.active = props.hearAboutUs?.active ?? true;
  nameError.value = "";
};
// Clear the duplicate name error when the user changes the name.
const clearNameError = () => {
  if (nameError.value) {
    nameError.value = "";
  }
};
// Reset the form fields and validation.
const reset = () => {
  form.name = "";
  form.active = true;
  nameError.value = "";
  formRef.value?.resetValidation();
};
// Validate and save the Hear About Us record.
const submit = async ({ clearDraft } = {}) => {
  nameError.value = "";
  const valid = await formRef.value?.validate();
  if (!valid) {
    return;
  }
  saving.value = true;
  try {
    const payload = {
      name: form.name.trim(),
      active: form.active
    };
    // Update the existing Hear About Us record when editing.
    if (props.editing && props.hearAboutUs?.hearAboutUsId) {
      await hearAboutUsApi.update(props.hearAboutUs.hearAboutUsId,payload);
      notify.success("Hear About Us updated.");
    } else {
      await hearAboutUsApi.create(payload);
      notify.success("Hear About Us created.");
    }
    clearDraft?.();
    formOpen.value = false;
    reset();
    emit("saved");
  } catch (error) {
    // Handle duplicate-name validation from the API.
    if (error?.response?.status === 409) {
      nameError.value = "A Hear About Us record with this name already exists.";
      return;
    }
    // Get a user-friendly error message from the API.
    const message = getApiErrorMessage(error, "Unable to save Hear About Us.");
    notify.error(message);
  } finally {
    saving.value = false;
  }
};
</script>
