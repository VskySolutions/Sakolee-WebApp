<!-- UserEditDrawer.vue -->
<template>
  <div>
    <!-- User Edit Drawer Component -->
    <app-form-dialog v-model="open" title="Edit Staff" size="md" :saving="saving" @submit="submitForm" @cancel="resetForm">
      <q-form ref="formRef" greedy>
        <div class="row q-col-gutter-md q-mb-md">
          <app-text-field
            v-model="form.firstName" label="First Name *" class="col-12 col-sm-6"
            :rules="nameRules('First name', { required: true })"
          />
          <app-text-field
            v-model="form.lastName" label="Last Name *" class="col-12 col-sm-6"
            :rules="nameRules('Last name', { required: true })"
          />
        </div>
        <app-text-field
          v-model="form.email" type="email" label="Email *" required class="q-mb-md"
          hint="The user signs in with this email."
          :error="!!emailError" :error-message="emailError"
          :rules="[(v) => !!v || 'Email is required', (v) => /.+@.+\..+/.test(v) || 'Enter a valid email']"
        />
      </q-form>
    </app-form-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { userApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";
import { nameRules } from "utils/personName";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";

// Props and emits
const props = defineProps({
  userId: { type: [String, Number], default: null }
});
const emit = defineEmits(["updated"]);

const open = defineModel({ type: Boolean, default: false });

const notify = useNotify();
const saving = ref(false);
const emailError = ref("");
const formRef = ref(null);

// Reactive form data
const form = reactive({
  firstName: "",
  lastName: "",
  email: ""
});

// Reset form fields and errors
const resetForm = () => {
  form.firstName = "";
  form.lastName = "";
  form.email = "";
  emailError.value = "";
};

// Watch for changes in the open state to fetch user details when the drawer is opened
watch(open, async (isOpen) => {
  if (!isOpen || !props.userId) return;
  resetForm();
  saving.value = true;
  try {
    const userDetail = await userApi.get(props.userId);
    if (userDetail) {
      form.firstName = userDetail.firstName || "";
      form.lastName = userDetail.lastName || "";
      form.email = userDetail.email || "";
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    open.value = false;
  } finally {
    saving.value = false;
  }
});

// Submit form data to update user details
const submitForm = async () => {
  emailError.value = "";
  if (!(await formRef.value?.validate())) return;

  saving.value = true;
  try {
    const payload = {
      firstName: form.firstName,
      lastName: form.lastName,
      email: form.email
    };

    await userApi.update(props.userId, payload);
    notify.success("Staff updated successfully.");
    open.value = false;
    emit("updated");
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      emailError.value = "This email is already in use.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>