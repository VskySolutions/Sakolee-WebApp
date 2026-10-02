<template>
  <q-page padding>
    <div class="q-mx-auto" style="max-width: 560px;">
      <div class="text-h5 text-weight-bold q-mb-md">Change Password</div>
      <q-card flat bordered class="account-card">
        <q-card-section class="row items-center q-gutter-sm">
          <q-icon name="o_lock" color="primary" size="sm" />
          <div class="text-subtitle1 text-weight-medium">Update your password</div>
        </q-card-section>
        <q-separator />
        <!-- Shown only on the forced change (first sign-in / after a reset), so the user knows why they landed here. -->
        <q-card-section v-if="authStore.mustChangePassword" class="q-pb-none">
          <q-banner dense class="bg-orange-1 text-orange-10 forced-change-note">
            <template #avatar><q-icon name="o_shield" color="warning" /></template>
            <div class="text-weight-medium">Note: For security purposes, please change your password.</div>
            <div class="text-caption">
              You signed in with a temporary password. Set a new password of your own to continue using the application.
            </div>
          </q-banner>
        </q-card-section>
        <change-password-form
          :cancel-to="{ name: 'account' }" autofocus @changed="submitted = true"
        />
      </q-card>
    </div>
  </q-page>
</template>

<script setup>
// The dedicated change-password screen, also used as the forced-change gate after a first sign-in or a
// password reset.
import { ref } from "vue";
import { onBeforeRouteLeave } from "vue-router";
import { useAuthStore } from "stores/auth";
import { useNotify } from "composables/useNotify";
import ChangePasswordForm from "components/account/ChangePasswordForm.vue";

const authStore = useAuthStore();
const { notifyWarning } = useNotify();

// Set from the form's `changed` event, which fires before it redirects to the login screen — otherwise the
// guard below would block its own success navigation.
const submitted = ref(false);

// AC-UI-003.4: block leaving the forced password-change screen until submitted.
onBeforeRouteLeave((to) => {
  if (authStore.mustChangePassword && !submitted.value && to.name !== "change_password") {
    notifyWarning("Please set a new password before continuing.");
    return false;
  }
  return true;
});
</script>

<style scoped>
.account-card {
  border-radius: 16px;
}
.forced-change-note {
  border-radius: 8px;
}
</style>
