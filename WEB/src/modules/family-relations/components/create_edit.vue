<template>
  <!-- 
    ============================================================
    Create Family Relation Drawer Component
    ============================================================
  -->
  <app-form-drawer
    v-model="isOpen"
    title="Create Family Relation"
    :saving="saving"
    save-label="Create"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-form ref="formRef" greedy>
      <!-- Relation name input field with required and length validation rules -->
      <app-text-field
        v-model="form.name"
        label="Relation Name"
        required
        class="q-mb-md"
        :rules="[
          (v) => !!v?.trim() || 'Relation name is required',
          (v) => !v || v.trim().length <= 100 || 'Name cannot exceed 100 characters'
        ]"
      />

      <!-- Active status toggle field -->
      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, computed } from "vue";
import { familyRelationApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppTextField from "components/common/AppTextField.vue";

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  }
});

const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const saving = ref(false);
const formRef = ref(null);

/*
 * Reactive form state for creating a new relation.
 */
const form = reactive({
  name: "",
  active: true
});

/*
 * Computed property for two-way binding of the drawer's open/close state.
 */
const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

/*
 * Reset form input fields and clear validation states.
 */
const resetForm = () => {
  form.name = "";
  form.active = true;
  formRef.value?.resetValidation?.();
};

/*
 * Handle form submission, execute API creation call, and manage duplicate errors.
 */
const submitForm = async ({ clearDraft } = {}) => {
  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: form.name.trim(),
      active: form.active
    };

    await familyRelationApi.create(payload);
    notify.success("Family relation created successfully.");

    clearDraft?.();
    isOpen.value = false;
    resetForm();
    emit("saved");
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A relation with this name already exists.");
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>