export default [
  {
    path: "/classes",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "classes",
        component: () => import("modules/class/pages/index.vue"),
        meta: { requiresAuth: true, permissions: ["classes.read"], title: "Classes" }
      },
      {
        path: "create",
        name: "class_create",
        component: () => import("modules/class/pages/create.vue"),
        meta: { requiresAuth: true, permissions: ["classes.read"], title: "Add Class" }
      },
      {
        path: ":id/edit",
        name: "class_edit",
        component: () => import("modules/class/pages/edit.vue"),
        meta: { requiresAuth: true, permissions: ["classes.read"], title: "Edit Class" }
      },
      {
        path: ":id",
        name: "class_detail",
        component: () => import("modules/class/pages/detail.vue"),
        meta: { requiresAuth: true, permissions: ["classes.read"], title: "View Class" }
      }
    ]
  }
];
