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
        path: ":id",
        name: "class_detail",
        component: () => import("modules/class/components/detail.vue"),
        meta: { requiresAuth: true, permissions: ["classes.read"], title: "View Class" }
      }
    ]
  }
];
