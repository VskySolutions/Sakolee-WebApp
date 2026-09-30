<template>
  <app-form-dialog v-model="dialogOpen" :title="id ? 'Edit Location' : 'Create Location'" :saving="saving" :save-label="id ? 'Save' : 'Create'" size="sm" @submit="submitForm" @cancel="closeDialog">
    <q-form ref="formRef" greedy @submit.prevent.stop="submitForm">
      <app-text-field
        v-model="form.name"
        label="Name"
        required
        maxlength="100"
        autofocus
        :error="v$.name.$error"
        :error-message="v$.name.$errors[0]?.$message"
        @blur="v$.name.$touch"
      />

      <q-toggle v-model="form.active" label="Active" />
    </q-form>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import useVuelidate from "@vuelidate/core";
import { required, helpers, maxLength } from "@vuelidate/validators";
import { locationApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  id: { type: [String, Number], default: null }
});

const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();

const formRef = ref(null);
const saving = ref(false);
const loading = ref(false);

const dialogOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit("update:modelValue", value)
});

const form = reactive({
  name: "",
  active: true
});

const rules = {
  name: {
    required: helpers.withMessage("Location name is required", required),
    maxLength: helpers.withMessage("Location name cannot exceed 100 characters", maxLength(100))
  }
};

const v$ = useVuelidate(rules, form, { $lazy: true, $autoDirty: true });

const resetForm = () => {
  form.name = "";
  form.active = true;
  v$.value.$reset();
};

const closeDialog = () => {
  if (saving.value) {
    return;
  }

  dialogOpen.value = false;
  resetForm();
};

const loadLocation = async () => {
  if (!props.id) {
    resetForm();
    return;
  }
  loading.value = true;
  try {
    const location = await locationApi.get(props.id);
    form.name = location?.name || "";
    form.active = location?.active ?? true;
    v$.value.$reset();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    dialogOpen.value = false;
  } finally {
    loading.value = false;
  }
};

const submitForm = async () => {
  const valid = await v$.value.$validate();
  if (!valid) {
    return;
  }
  saving.value = true;
  try {
    const payload = { name: form.name.trim(), active: form.active };
    if (props.id) {
      await locationApi.update(props.id, payload);
      notify.success("Location updated.");
    } else {
      await locationApi.create(payload);
      notify.success("Location created.");
    }
    emit("saved");
    dialogOpen.value = false;
    resetForm();
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A location with this name already exists.");
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};

watch(
  () => props.modelValue,
  (value) => {
    if (value) {
      loadLocation();
    } else {
      resetForm();
    }
  }
);

watch(
  () => props.id,
  () => {
    if (props.modelValue) {
      loadLocation();
    }
  }
);
</script>
