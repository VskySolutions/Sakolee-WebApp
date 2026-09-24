export default [
  {
    path: "/class-categories",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "class_categories",
        component: () => import("modules/class-category/pages/index.vue"),
        meta: {
          requiresAuth: true,
          permissions: ["classCategories.read"],
          title: "Class Categories"
        }
      }
    ]
  }
];
