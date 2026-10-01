<template>
  <!-- Dialog used for creating and editing an Account Type record -->
  <app-form-dialog
    v-model="formOpen"
    :title="editing ? 'Edit Account Type' : 'Create Account Type'"
    :saving="saving"
    :save-label="editing ? 'Save' : 'Create'"
    size="sm"
    @submit="submit"
    @cancel="reset"
  >
    <q-form ref="formRef" greedy>
      <!-- Account Type name -->
      <app-text-field
        v-model="form.name"
        label="Name"
        required
        class="q-mb-md"
        :error="!!nameError"
        :error-message="nameError"
        :rules="[
          (v) => !!v?.trim() || 'Account Type name is required'
        ]"
        @update:model-value="clearNameError"
      />
      <!-- Active status -->
      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-dialog>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { accountTypeApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";

// Props and emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editing: { type: Boolean, default: false },
  accountType: { type: Object, default: null }
});
const emit = defineEmits(["update:modelValue", "saved"]);
const notify = useNotify();
const formOpen = ref(props.modelValue);
const formRef = ref(null);
const saving = ref(false);
const nameError = ref("");
const form = reactive({ name: "", active: true });
/*
 * Watch dialog state from parent.
 */
watch(
  () => props.modelValue,
  (value) => {
    formOpen.value = value;
    if (value) {
      loadForm();
    }
  }
);
/*
 * Send dialog state back to parent.
 */
watch(formOpen, (value) => {
  emit("update:modelValue", value);
});
/*
 * Load existing Account Type data when editing.
 */
const loadForm = () => {
  form.name = props.accountType?.name || "";
  form.active = props.accountType?.active ?? true;
  nameError.value = "";
};
/*
 * Clear duplicate-name error when name changes.
 */
const clearNameError = () => {
  if (nameError.value) {
    nameError.value = "";
  }
};
/*
 * Reset form.
 */
const reset = () => {
  form.name = "";
  form.active = true;
  nameError.value = "";
  formRef.value?.resetValidation();
};
/*
 * Create / update Account Type.
 */
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
    /*
     * Update existing record.
     */
    if (props.editing && props.accountType?.accountTypeId) {
      await accountTypeApi.update(props.accountType.accountTypeId, payload);
      notify.success("Account Type updated.");
    } else {
      /*
       * Create new record.
       */
      await accountTypeApi.create(payload);
      notify.success("Account Type created.");
    }
    clearDraft?.();
    formOpen.value = false;
    reset();
    emit("saved");
  } catch (error) {
    /*
     * Handle duplicate Account Type name.
     */
    if (error?.response?.status === 409) {
      nameError.value = "An Account Type record with this name already exists.";
      return;
    }
    const message = getApiErrorMessage(error, "Unable to save Account Type.");
    notify.error(message);
  } finally {
    saving.value = false;
  }
};
</script>
