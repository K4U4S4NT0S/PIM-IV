const $ = (selector, root = document) => root.querySelector(selector);
const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];

const API_BASE = localStorage.getItem("foodsmart_api") || "http://localhost:5077/api";
const TOKEN_KEY = "foodsmart-token";
const USER_KEY = "foodsmart-user";
const LEGACY_TOKEN_KEY = "foodsmart_token";
const LEGACY_USER_KEY = "foodsmart_user";
const LAST_ORDER_KEY = "foodsmart_last_order";

function getStoredToken() {
  const token = localStorage.getItem(TOKEN_KEY) || localStorage.getItem(LEGACY_TOKEN_KEY);
  if (!token || token === "demo-token") return null;
  return token;
}

function cleanupDemoAuth() {
  if (localStorage.getItem(TOKEN_KEY) === "demo-token") localStorage.removeItem(TOKEN_KEY);
  if (localStorage.getItem(LEGACY_TOKEN_KEY) === "demo-token") localStorage.removeItem(LEGACY_TOKEN_KEY);
}

const money = (value) =>
  Number(value || 0).toLocaleString("pt-BR", {
    style: "currency",
    currency: "BRL",
  });

const normalizeCategory = (category = "") => {
  const value = String(category).toLowerCase();
  if (value.includes("bebida")) return "bebida";
  if (value.includes("adicional") || value.includes("acompanhamento")) return "adicional";
  return "lanche";
};

const normalizeProduct = (product) => ({
  id: product.id,
  type: normalizeCategory(product.category || product.type),
  category: normalizeCategory(product.category || product.type),
  name: product.name,
  price: Number(product.price || 0),
  img: product.imageUrl || product.img || "https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=1000&q=88",
  imageUrl: product.imageUrl || product.img || "",
  desc: product.description || product.desc || "Produto artesanal FoodSmart.",
  description: product.description || product.desc || "Produto artesanal FoodSmart.",
  emoji: product.emoji || "🍔",
  ing: product.ingredients || product.ing || ["Receita da casa", "Ingredientes selecionados"],
  isFeatured: Boolean(product.isFeatured),
  isActive: product.isActive !== false,
});

const fallbackProducts = [
  { id: 1, type: "lanche", name: "X-Tudo Prime", price: 39.9, img: "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=1000&q=88", desc: "Blend bovino, cheddar, bacon, ovo, salada fresca e molho especial da casa.", ing: ["Blend bovino", "Cheddar", "Bacon", "Ovo", "Alface", "Tomate"] },
  { id: 2, type: "lanche", name: "Double Smash", price: 34.9, img: "https://images.unsplash.com/photo-1553979459-d2229ba7433b?auto=format&fit=crop&w=1000&q=88", desc: "Dois discos smash prensados na chapa com cheddar duplo, picles e maionese defumada.", ing: ["2 smash", "Cheddar", "Picles", "Maionese defumada"] },
  { id: 3, type: "lanche", name: "Chicken Crispy", price: 29.9, img: "https://images.unsplash.com/photo-1606755962773-d324e2d2404c?auto=format&fit=crop&w=1000&q=88", desc: "Frango crocante, queijo, salada e molho levemente picante.", ing: ["Frango", "Queijo", "Alface", "Molho spicy"] },
  { id: 4, type: "lanche", name: "Bacon Melt", price: 36.9, img: "https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=1000&q=88", desc: "Hambúrguer artesanal, bacon crocante, cheddar cremoso e cebola caramelizada.", ing: ["Blend", "Bacon", "Cheddar", "Cebola"] },
  { id: 5, type: "lanche", name: "Brasa Burger", price: 41.9, img: "https://images.unsplash.com/photo-1586190848861-99aa4a171e90?auto=format&fit=crop&w=1000&q=88", desc: "Carne grelhada, queijo prato, barbecue premium e onion rings.", ing: ["Carne", "Queijo", "Barbecue", "Onion rings"] },
  { id: 6, type: "bebida", name: "Coca-Cola Lata", price: 7.9, img: "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?auto=format&fit=crop&w=1000&q=88", desc: "Refrigerante gelado para acompanhar o combo.", ing: ["350ml", "Gelada"] },
  { id: 7, type: "bebida", name: "Limonada da Casa", price: 12.9, img: "https://images.unsplash.com/photo-1621263764928-df1444c5e859?auto=format&fit=crop&w=1000&q=88", desc: "Limonada fresca com toque cítrico.", ing: ["Limão", "Gelo", "Hortelã"] },
  { id: 8, type: "bebida", name: "Milkshake Chocolate", price: 18.9, img: "https://images.unsplash.com/photo-1572490122747-3968b75cc699?auto=format&fit=crop&w=1000&q=88", desc: "Milkshake cremoso de chocolate.", ing: ["Sorvete", "Leite", "Chocolate"] },
  { id: 9, type: "adicional", name: "Batata Rústica", price: 14.9, img: "https://images.unsplash.com/photo-1576107232684-1279f390859f?auto=format&fit=crop&w=1000&q=88", desc: "Batata crocante com tempero especial.", ing: ["Batata", "Páprica", "Ervas"] },
  { id: 10, type: "adicional", name: "Onion Rings", price: 15.9, img: "https://images.unsplash.com/photo-1639024471283-03518883512d?auto=format&fit=crop&w=1000&q=88", desc: "Anéis de cebola empanados.", ing: ["Cebola", "Empanado"] },
  { id: 11, type: "adicional", name: "Cheddar Extra", price: 6.9, img: "https://images.unsplash.com/photo-1486297678162-eb2a19b0a32d?auto=format&fit=crop&w=1000&q=88", desc: "Porção extra de cheddar cremoso.", ing: ["Cheddar"] },
];

let products = fallbackProducts.map(normalizeProduct);
let burgers = [];
let drinks = [];
let extras = [];

const ingredients = [
  "Sem cebola",
  "Sem tomate",
  "Ponto bem passado",
  "Carne ao ponto",
  "Molho separado",
  "Bacon extra",
  "Cheddar extra",
  "Picles",
  "Jalapeño",
  "Alface extra",
  "Cebola caramelizada",
  "Ovo extra",
];

function splitProducts() {
  burgers = products.filter((product) => product.type === "lanche");
  drinks = products.filter((product) => product.type === "bebida");
  extras = products.filter((product) => product.type === "adicional");
}

splitProducts();

async function api(path, options = {}) {
  const token = getStoredToken();
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {}),
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  const text = await response.text();
  const data = text ? JSON.parse(text) : null;

  if (!response.ok) {
    throw new Error(data?.message || "Erro de comunicação com a API.");
  }

  return data;
}

async function loadProducts(includeInactive = false) {
  try {
    const data = await api(`/products${includeInactive ? "?includeInactive=true" : ""}`);
    products = data.map(normalizeProduct);
    splitProducts();
    return products;
  } catch (error) {
    console.warn("Usando produtos locais porque a API não respondeu:", error.message);
    splitProducts();
    return products;
  }
}

function getUser() {
  try {
    const raw = localStorage.getItem(USER_KEY) || localStorage.getItem(LEGACY_USER_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}
function isAdmin() {
  const user = getUser();
  return Boolean(user && getStoredToken() && user.role === "Admin");
}

function isLoggedIn() {
  return Boolean(getUser() && getStoredToken());
}

function requireLogin(message = "Faça login para continuar.") {
  if (isLoggedIn()) return true;
  toast(message, "error");
  setTimeout(() => (location.href = "login.html"), 900);
  return false;
}

function requireAdmin() {
  if (isAdmin()) return true;
  return false;
}

function renderAccessDenied(target, title = "Acesso restrito", message = "Esta área é exclusiva para administradores.") {
  if (!target) return;
  target.innerHTML = `
    <section class="container access-denied reveal show">
      <span class="pill">permissão limitada</span>
      <h1>${title}</h1>
      <p>${message}</p>
      <div class="access-actions">
        <a class="btn brand" href="usuario.html">Ir para minha conta</a>
        <a class="btn ghost" href="index.html">Voltar ao site</a>
      </div>
    </section>`;
}


function setSession(payload) {
  if (!payload?.token || !payload?.user) {
    throw new Error("Resposta de login inválida.");
  }

  localStorage.setItem(TOKEN_KEY, payload.token);
  localStorage.setItem(USER_KEY, JSON.stringify(payload.user));
  localStorage.removeItem(LEGACY_TOKEN_KEY);
  localStorage.removeItem(LEGACY_USER_KEY);
}

function logout() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  localStorage.removeItem(LEGACY_TOKEN_KEY);
  localStorage.removeItem(LEGACY_USER_KEY);
  localStorage.removeItem(LAST_ORDER_KEY);
  location.href = "login.html";
}

function toast(message, type = "ok") {
  let el = $(".toast");
  if (!el) {
    el = document.createElement("div");
    el.className = "toast";
    document.body.appendChild(el);
  }
  el.textContent = message;
  el.dataset.type = type;
  el.classList.add("show");
  clearTimeout(toast.timer);
  toast.timer = setTimeout(() => el.classList.remove("show"), 3600);
}

function initBase() {
  cleanupDemoAuth();
  const root = document.documentElement;
  const savedTheme = localStorage.getItem("theme");
  const user = getUser();
  const token = getStoredToken();

  if (savedTheme) {
    root.setAttribute("data-theme", savedTheme);
  }

  $$('[data-theme-toggle]').forEach((button) => {
    button.addEventListener("click", () => {
      const isDark = root.getAttribute("data-theme") === "dark";
      const nextTheme = isDark ? "light" : "dark";
      root.setAttribute("data-theme", nextTheme);
      localStorage.setItem("theme", nextTheme);
    });
  });

  $$(".menu").forEach((menu) => {
    const adminLink = [...menu.querySelectorAll('a')].find((a) => a.getAttribute("href") === "admin.html");
    const userLink = [...menu.querySelectorAll('a')].find((a) => a.getAttribute("href") === "usuario.html");

    if (adminLink && user?.role !== "Admin") {
      adminLink.remove();
    }

    if (user && token && !userLink) {
      const account = document.createElement("a");
      account.href = user.role === "Admin" ? "admin.html" : "usuario.html";
      account.textContent = user.role === "Admin" ? "Admin" : "Minha conta";
      if (location.pathname.endsWith(account.getAttribute("href"))) account.classList.add("active");
      menu.appendChild(account);
    }
  });

  $$(".nav-actions").forEach((actions) => {
    const loginLink = [...actions.querySelectorAll('a')].find((a) => a.getAttribute("href") === "login.html");
    const existingLogout = actions.querySelector("[data-logout]");

    if (user && token && loginLink) {
      const firstName = (user.name || "Minha conta").split(" ")[0];
      loginLink.innerHTML = `<span class="user-dot"></span>${user.role === "Admin" ? "Admin" : firstName}`;
      loginLink.href = user.role === "Admin" ? "admin.html" : "usuario.html";
      loginLink.classList.add("logged-user");
      loginLink.title = `Logado como ${user.name || user.email}`;
    }

    if (user && token && !existingLogout) {
      const logoutButton = document.createElement("button");
      logoutButton.className = "btn ghost mini-logout";
      logoutButton.textContent = "Sair";
      logoutButton.dataset.logout = "true";
      logoutButton.type = "button";
      logoutButton.addEventListener("click", logout);
      actions.appendChild(logoutButton);
    }
  });

  $$("[data-admin-only]").forEach((element) => {
    if (user?.role !== "Admin") element.remove();
  });

  $$("[data-client-only]").forEach((element) => {
    if (!user || user.role === "Admin") element.remove();
  });

  $(".hamb")?.addEventListener("click", () => $(".menu")?.classList.toggle("open"));

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) entry.target.classList.add("show");
      });
    },
    { threshold: 0.12 }
  );

  $$(".reveal").forEach((element) => observer.observe(element));
}

function initAuth() {
  const form = $("#authForm");
  if (!form) return;

  const isRegister = location.pathname.includes("cadastro");

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const inputs = $$('input', form);
    const button = $("button[type='submit'], .btn:not(.ghost)", form);

    const payload = isRegister
      ? {
          name: inputs[0]?.value.trim(),
          email: inputs[1]?.value.trim(),
          phone: inputs[2]?.value.trim(),
          password: inputs[3]?.value,
        }
      : {
          email: inputs[0]?.value.trim(),
          password: inputs[1]?.value,
        };

    try {
      if (button) button.textContent = isRegister ? "Criando conta..." : "Entrando...";
      const result = await api(isRegister ? "/auth/register" : "/auth/login", {
        method: "POST",
        body: JSON.stringify(payload),
      });
      setSession(result);
      toast(isRegister ? "Conta criada com sucesso." : "Login realizado com sucesso.");
      location.href = result.user?.role === "Admin" ? "admin.html" : "usuario.html";
    } catch (error) {
      toast(error.message, "error");
    } finally {
      if (button) button.textContent = isRegister ? "Criar conta →" : "Entrar na conta →";
    }
  });
}

function productIngredients(product) {
  return product.ing?.length ? product.ing : ["Receita artesanal", "Ingredientes selecionados", "Padrão FoodSmart"];
}

function initCatalog() {
  const grid = $("#catalogGrid");
  if (!grid) return;

  const openModal = (product) => {
    $("#modalImg").src = product.img;
    $("#modalTitle").textContent = product.name;
    $("#modalDesc").textContent = product.desc;
    $("#modalPrice").textContent = money(product.price);
    $("#modalCategory").textContent = product.type;
    $("#modalIngredients").innerHTML = productIngredients(product).map((ingredient) => `<li>${ingredient}</li>`).join("");
    $("#productModal").classList.add("open");
  };

  const render = (filter = "all") => {
    const visible = products.filter((product) => filter === "all" || product.type === filter);
    grid.innerHTML = visible
      .map(
        (product) => `
          <article class="catalog-card" data-type="${product.type}">
            <div class="catalog-img"><img src="${product.img}" alt="${product.name}" loading="lazy"></div>
            <div class="catalog-body">
              <span class="tag">${product.type}</span>
              <h3>${product.name}</h3>
              <p>${product.desc}</p>
              <div class="ingredients-line">
                ${productIngredients(product).slice(0, 4).map((ingredient) => `<span>${ingredient}</span>`).join("")}
              </div>
              <div class="card-row">
                <strong class="price">${money(product.price)}</strong>
                <button class="btn ghost view-product" data-id="${product.id}" type="button">Ver sobre →</button>
              </div>
            </div>
          </article>`
      )
      .join("");

    $$(".view-product").forEach((button) => {
      button.addEventListener("click", () => openModal(products.find((product) => product.id === Number(button.dataset.id))));
    });
  };

  $$('[data-close-modal]').forEach((button) => button.addEventListener("click", () => $("#productModal")?.classList.remove("open")));
  $$(".filter-btn").forEach((button) => {
    button.addEventListener("click", () => {
      $$(".filter-btn").forEach((item) => item.classList.remove("active"));
      button.classList.add("active");
      render(button.dataset.filter);
    });
  });

  render();
}

function initOrder() {
  if (!$("#burgerChoices")) return;

  const state = {
    burgers: [],
    drinks: [],
    extras: [],
    ingredients: [],
    note: "",
    lastOrder: null,
  };

  if (burgers[0]) state.burgers = [burgers[0]];

  const createChoiceCard = (product, type, active = false) => `
    <button class="order-premium-card ${active ? "active" : ""}" data-kind="${type}" data-id="${product.id}" type="button">
      <span class="select-mark">✓</span>
      <span class="card-glow"></span>
      <span class="image-wrap"><img src="${product.img}" alt="${product.name}" loading="lazy" referrerpolicy="no-referrer"></span>
      <span class="order-card-content">
        <small>${type === "burger" ? "Lanche" : type === "drink" ? "Bebida" : "Adicional"}</small>
        <strong>${product.name}</strong>
        <em>${product.desc}</em>
        <b>${money(product.price)}</b>
      </span>
    </button>`;

  $("#burgerChoices").innerHTML = burgers.map((product, index) => createChoiceCard(product, "burger", index === 0)).join("");
  $("#drinkChoices").innerHTML = drinks.map((product) => createChoiceCard(product, "drink")).join("");
  $("#extraChoices").innerHTML = extras.map((product) => createChoiceCard(product, "extra")).join("");
  $("#ingredientChips").innerHTML = ingredients.map((ingredient) => `<button class="chip" data-ing="${ingredient}" type="button">${ingredient}</button>`).join("");

  const allSelected = () => [...state.burgers, ...state.drinks, ...state.extras];

  const update = () => {
    const selectedProducts = allSelected();
    const total = selectedProducts.reduce((sum, product) => sum + product.price, 0);
    const comboCount = state.drinks.length + state.extras.length + state.ingredients.length;
    const mainBurger = state.burgers[0] || selectedProducts[0] || burgers[0] || products[0];

    if (mainBurger) {
      $("#receiptImg").src = mainBurger.img;
      $("#receiptName").textContent = state.burgers.length > 1 ? `${state.burgers.length} lanches selecionados` : mainBurger.name;
      $("#receiptDesc").textContent = state.burgers.length > 1 ? "Seu pedido tem múltiplos lanches. Confira o resumo antes de finalizar." : mainBurger.desc;
    }

    $("#receiptItems").innerHTML = `
      <div class="receipt-meta">
        <span>${selectedProducts.length} item(ns)</span>
        <span>${comboCount} personalização(ões)</span>
      </div>
      ${selectedProducts
        .map(
          (product) => `
            <div class="receipt-line">
              <span>${product.name}</span>
              <strong>${money(product.price)}</strong>
            </div>`
        )
        .join("")}
      ${state.ingredients.length ? `<div class="receipt-notes"><strong>Ingredientes:</strong> ${state.ingredients.join(", ")}</div>` : ""}
      ${state.note ? `<div class="receipt-notes"><strong>Observação:</strong> ${state.note}</div>` : ""}
      ${state.lastOrder ? `<div class="live-status"><strong>Pedido #${state.lastOrder.id}</strong><span>${state.lastOrder.status}</span></div>` : ""}
    `;
    $("#receiptTotal").textContent = money(total);
  };

  $$(".order-premium-card").forEach((button) => {
    button.addEventListener("click", () => {
      const kind = button.dataset.kind;
      const id = Number(button.dataset.id);
      const list = kind === "burger" ? state.burgers : kind === "drink" ? state.drinks : state.extras;
      const item = products.find((product) => product.id === id);
      const exists = list.find((product) => product.id === id);

      if (!item) return;
      button.classList.toggle("active");

      if (exists) {
        list.splice(list.indexOf(exists), 1);
      } else {
        list.push(item);
      }

      update();
    });
  });

  $$(".chip").forEach((button) => {
    button.addEventListener("click", () => {
      const ingredient = button.dataset.ing;
      button.classList.toggle("active");
      if (state.ingredients.includes(ingredient)) {
        state.ingredients.splice(state.ingredients.indexOf(ingredient), 1);
      } else {
        state.ingredients.push(ingredient);
      }
      update();
    });
  });

  $("#obs")?.addEventListener("input", (event) => {
    state.note = event.target.value.trim();
    update();
  });

  const checkoutForm = $(".checkout-form");
  checkoutForm?.addEventListener("submit", (event) => event.preventDefault());
  $(".checkout-form .btn")?.addEventListener("click", async () => {
    if (!requireLogin("Faça login para salvar seu pedido no SQL Server.")) return;
    const inputs = $$("input", checkoutForm);
    const selectedProducts = allSelected();

    if (!selectedProducts.length) {
      toast("Selecione pelo menos um item.", "error");
      return;
    }

    const customerName = inputs[0]?.value.trim();
    const customerPhone = inputs[1]?.value.trim();
    const customerAddress = inputs[2]?.value.trim();

    if (!customerName || !customerPhone || !customerAddress) {
      toast("Preencha nome, WhatsApp e endereço.", "error");
      return;
    }

    const groupedItems = selectedProducts.reduce((acc, product) => {
      acc[product.id] = (acc[product.id] || 0) + 1;
      return acc;
    }, {});

    const payload = {
      customerName,
      customerPhone,
      customerAddress: `${customerAddress}${state.ingredients.length ? ` | Ingredientes: ${state.ingredients.join(", ")}` : ""}${state.note ? ` | Obs: ${state.note}` : ""}`,
      items: Object.entries(groupedItems).map(([productId, quantity]) => ({ productId: Number(productId), quantity })),
    };

    try {
      const order = await api("/orders", {
        method: "POST",
        body: JSON.stringify(payload),
      });
      state.lastOrder = order;
      localStorage.setItem(LAST_ORDER_KEY, JSON.stringify(order));
      update();
      showOrderModal(order);
      watchOrderStatus(order.id, state, update);
    } catch (error) {
      toast(error.message, "error");
    }
  });

  const savedOrder = localStorage.getItem(LAST_ORDER_KEY);
  if (savedOrder) {
    try {
      state.lastOrder = JSON.parse(savedOrder);
      watchOrderStatus(state.lastOrder.id, state, update);
    } catch {}
  }

  update();
}

function showOrderModal(order) {
  let modal = $("#orderSuccessModal");
  if (!modal) {
    modal = document.createElement("div");
    modal.id = "orderSuccessModal";
    modal.className = "modal open";
    document.body.appendChild(modal);
  }

  modal.innerHTML = `
    <div class="modal-backdrop" data-close-order></div>
    <div class="modal-card order-modal-card">
      <button class="modal-close" data-close-order type="button">×</button>
      <span class="pill">pedido salvo no SQL Server</span>
      <h2>Pedido #${order.id} recebido</h2>
      <p>Seu pedido foi registrado no banco e já aparece no painel administrativo.</p>
      <div class="receipt-line"><span>Status</span><strong>${order.status}</strong></div>
      <div class="receipt-line"><span>Total</span><strong>${money(order.total)}</strong></div>
      <button class="btn brand wide" data-close-order type="button">Entendi</button>
    </div>`;

  $$('[data-close-order]', modal).forEach((button) => {
    button.addEventListener("click", () => modal.classList.remove("open"));
  });
}

function watchOrderStatus(orderId, state, update) {
  if (!orderId) return;
  clearInterval(watchOrderStatus.timer);
  watchOrderStatus.timer = setInterval(async () => {
    try {
      const current = await api(`/orders/${orderId}/status`);
      if (current) {
        state.lastOrder = current;
        localStorage.setItem(LAST_ORDER_KEY, JSON.stringify(current));
        update();
      }
    } catch {
      clearInterval(watchOrderStatus.timer);
    }
  }, 6000);
}

function initQuickOrderForm() {
  const form = $("#orderForm");
  if (!form) return;

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const name = $("#nome")?.value.trim();
    const itemName = $("#item")?.value;
    const product = products.find((item) => item.name === itemName) || products[0];

    if (!name || !product) {
      toast("Preencha os dados do pedido.", "error");
      return;
    }

    try {
      await api("/orders", {
        method: "POST",
        body: JSON.stringify({
          customerName: name,
          customerPhone: "Não informado",
          customerAddress: "Retirada no balcão",
          items: [{ productId: product.id, quantity: 1 }],
        }),
      });
      toast("Pedido rápido salvo no SQL Server.");
      form.reset();
    } catch (error) {
      toast(error.message, "error");
    }
  });
}

function statusClass(status) {
  return String(status || "").toLowerCase().replaceAll(" ", "-").replace("í", "i");
}

async function initAdmin() {
  const adminPage = $("[data-admin-page]") || $(".admin-premium");
  const dashboard = $(".admin-dashboard");
  if (!adminPage || !dashboard) return;

  const user = getUser();

  if (!getStoredToken()) {
    renderAccessDenied(adminPage, "Faça login como administrador", "Você precisa entrar com uma conta administradora para acessar produtos, pedidos e métricas.");
    return;
  }

  if (user?.role !== "Admin") {
    renderAccessDenied(adminPage, "Acesso negado", "Sua conta é do tipo Cliente. Você pode fazer pedidos e acompanhar seus próprios status, mas não pode gerenciar produtos, usuários ou pedidos de outras pessoas.");
    return;
  }

  dashboard.classList.remove("hidden");
  await loadAdminData();

  $("#productForm")?.addEventListener("submit", saveProduct);
  $("#clearProductForm")?.addEventListener("click", clearProductForm);
}

async function loadAdminData() {
  try {
    const [productData, orders, analytics] = await Promise.all([
      api("/products?includeInactive=true"),
      api("/orders"),
      api("/analytics/summary"),
    ]);
    products = productData.map(normalizeProduct);
    splitProducts();
    renderAdminStats(analytics);
    renderAdminProducts(products);
    renderAdminOrders(orders);
    clearInterval(loadAdminData.timer);
    loadAdminData.timer = setInterval(refreshAdminOrders, 5000);
  } catch (error) {
    toast(error.message, "error");
  }
}

async function refreshAdminOrders() {
  if (!$("#adminOrders")) return;
  try {
    const [orders, analytics] = await Promise.all([api("/orders"), api("/analytics/summary")]);
    renderAdminStats(analytics);
    renderAdminOrders(orders);
  } catch (error) {
    console.warn(error.message);
  }
}

function renderAdminStats(data) {
  let stats = $("#adminStats");
  if (!stats) {
    stats = document.createElement("div");
    stats.id = "adminStats";
    stats.className = "admin-stats-premium";
    $(".admin-dashboard")?.prepend(stats);
  }

  stats.innerHTML = `
    <article><span>Faturamento</span><strong>${money(data.revenue)}</strong></article>
    <article><span>Pedidos</span><strong>${data.orderCount}</strong></article>
    <article><span>Ticket médio</span><strong>${money(data.averageTicket)}</strong></article>
    <article><span>Pico de venda</span><strong>${data.peakHour}</strong></article>
  `;
}

function renderAdminProducts(items) {
  const list = $("#adminProducts");
  if (!list) return;

  list.innerHTML = items
    .map(
      (product) => `
        <article class="admin-list-item ${product.isActive ? "" : "muted"}">
          <img src="${product.img}" alt="${product.name}">
          <div>
            <strong>${product.name}</strong>
            <span>${product.type} • ${money(product.price)} • ${product.isActive ? "ativo" : "inativo"}</span>
          </div>
          <div class="admin-actions">
            <button class="btn ghost" data-edit-product="${product.id}" type="button">Editar</button>
            <button class="btn ghost" data-disable-product="${product.id}" type="button">Desativar</button>
          </div>
        </article>`
    )
    .join("");

  $$('[data-edit-product]').forEach((button) => {
    button.addEventListener("click", () => fillProductForm(products.find((product) => product.id === Number(button.dataset.editProduct))));
  });

  $$('[data-disable-product]').forEach((button) => {
    button.addEventListener("click", async () => {
      try {
        await api(`/products/${button.dataset.disableProduct}`, { method: "DELETE" });
        toast("Produto desativado.");
        await loadAdminData();
      } catch (error) {
        toast(error.message, "error");
      }
    });
  });
}

function renderAdminOrders(orders) {
  const list = $("#adminOrders");
  if (!list) return;

  if (!orders.length) {
    list.innerHTML = "<p>Nenhum pedido recebido ainda.</p>";
    return;
  }

  const statuses = ["Recebido", "Preparando", "Saiu para entrega", "Concluído", "Cancelado"];

  list.innerHTML = orders
    .map(
      (order) => `
        <article class="order-admin-card status-${statusClass(order.status)}">
          <div class="order-admin-head">
            <div>
              <strong>Pedido #${order.id}</strong>
              <span>${new Date(order.createdAt).toLocaleString("pt-BR")}</span>
            </div>
            <b>${money(order.total)}</b>
          </div>
          <p><strong>Cliente:</strong> ${order.customerName} • ${order.customerPhone}</p>
          <p><strong>Endereço:</strong> ${order.customerAddress || "Retirada"}</p>
          <div class="order-items-mini">
            ${(order.items || [])
              .map((item) => `<span>${item.quantity}x ${item.product?.name || `Produto #${item.productId}`}</span>`)
              .join("")}
          </div>
          <label class="status-control">
            Status em tempo real
            <select class="input" data-order-status="${order.id}">
              ${statuses.map((status) => `<option value="${status}" ${status === order.status ? "selected" : ""}>${status}</option>`).join("")}
            </select>
          </label>
        </article>`
    )
    .join("");

  $$('[data-order-status]').forEach((select) => {
    select.addEventListener("change", async () => {
      try {
        await api(`/orders/${select.dataset.orderStatus}/status`, {
          method: "PATCH",
          body: JSON.stringify({ status: select.value }),
        });
        toast("Status atualizado.");
        await refreshAdminOrders();
      } catch (error) {
        toast(error.message, "error");
      }
    });
  });
}

async function saveProduct(event) {
  event.preventDefault();
  const id = $("#productId").value;
  const payload = {
    category: $("#productCategory").value,
    name: $("#productName").value.trim(),
    description: $("#productDescription").value.trim(),
    price: Number($("#productPrice").value),
    emoji: "🍔",
    imageUrl: $("#productImage").value.trim(),
    isFeatured: $("#productFeatured").checked,
    isActive: $("#productActive").checked,
  };

  try {
    await api(id ? `/products/${id}` : "/products", {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(payload),
    });
    toast(id ? "Produto atualizado." : "Produto cadastrado.");
    clearProductForm();
    await loadAdminData();
  } catch (error) {
    toast(error.message, "error");
  }
}

function fillProductForm(product) {
  if (!product) return;
  $("#productId").value = product.id;
  $("#productCategory").value = product.type;
  $("#productName").value = product.name;
  $("#productDescription").value = product.desc;
  $("#productPrice").value = product.price;
  $("#productImage").value = product.img;
  $("#productFeatured").checked = product.isFeatured;
  $("#productActive").checked = product.isActive;
  window.scrollTo({ top: $("#productForm").offsetTop - 120, behavior: "smooth" });
}

function clearProductForm() {
  $("#productForm")?.reset();
  $("#productId").value = "";
  $("#productActive").checked = true;
}


function initUserDashboard() {
  const page = $("#userDashboard");
  if (!page) return;

  const user = getUser();
  const token = getStoredToken();

  if (!user || !token) {
    toast("Faça login para acessar sua área.", "error");
    setTimeout(() => (location.href = "login.html"), 900);
    return;
  }

  const firstName = (user.name || "Cliente").split(" ")[0];
  $("#userGreeting") && ($("#userGreeting").textContent = `Olá, ${firstName}`);
  $("#userName") && ($("#userName").textContent = user.name || "Cliente FoodSmart");
  $("#userEmail") && ($("#userEmail").textContent = user.email || "-");
  $("#userRole") && ($("#userRole").textContent = user.role || "Cliente");
  $("#tokenPreview") && ($("#tokenPreview").textContent = `${token.slice(0, 18)}...${token.slice(-10)}`);

  const lastOrder = (() => {
    try {
      return JSON.parse(localStorage.getItem(LAST_ORDER_KEY) || "null");
    } catch {
      return null;
    }
  })();

  const orderBox = $("#userLastOrder");
  const myOrdersBox = $("#userOrdersList");

  const renderMyOrders = async () => {
    if (!myOrdersBox) return;

    try {
      const orders = await api("/orders/mine");
      if (!orders.length) {
        myOrdersBox.innerHTML = `<div class="empty-state"><strong>Nenhum pedido salvo ainda.</strong><span>Quando você finalizar um pedido, ele aparecerá aqui.</span></div>`;
        return;
      }

      myOrdersBox.innerHTML = orders
        .map((order) => `
          <article class="user-order-card mini">
            <div>
              <span class="pill">${order.status}</span>
              <h3>Pedido #${order.id}</h3>
              <p>${new Date(order.createdAt).toLocaleString("pt-BR")}</p>
              <div class="order-items-mini">
                ${(order.items || []).map((item) => `<span>${item.quantity}x ${item.product?.name || `Produto #${item.productId}`}</span>`).join("")}
              </div>
            </div>
            <strong>${money(order.total)}</strong>
          </article>`)
        .join("");
    } catch (error) {
      myOrdersBox.innerHTML = `<div class="empty-state"><strong>Não foi possível carregar seus pedidos.</strong><span>${error.message}</span></div>`;
    }
  };

  renderMyOrders();
  clearInterval(initUserDashboard.ordersTimer);
  initUserDashboard.ordersTimer = setInterval(renderMyOrders, 7000);

  if (orderBox) {
    if (!lastOrder) {
      orderBox.innerHTML = `
        <div class="empty-state">
          <strong>Nenhum pedido acompanhado ainda.</strong>
          <span>Faça um pedido para acompanhar o status em tempo real aqui.</span>
          <a class="btn brand" href="pedido.html">Fazer pedido agora →</a>
        </div>`;
    } else {
      orderBox.innerHTML = `
        <article class="user-order-card">
          <div>
            <span class="pill">último pedido</span>
            <h3>Pedido #${lastOrder.id}</h3>
            <p>${lastOrder.customerName || user.name} • ${lastOrder.customerPhone || "telefone não informado"}</p>
          </div>
          <strong>${money(lastOrder.total)}</strong>
          <div class="live-status"><strong>Status</strong><span>${lastOrder.status}</span></div>
        </article>`;

      const refresh = async () => {
        try {
          const current = await api(`/orders/${lastOrder.id}/status`);
          localStorage.setItem(LAST_ORDER_KEY, JSON.stringify(current));
          $("#userLastOrder .live-status span") && ($("#userLastOrder .live-status span").textContent = current.status);
        } catch (error) {
          console.warn(error.message);
        }
      };
      refresh();
      clearInterval(initUserDashboard.timer);
      initUserDashboard.timer = setInterval(refresh, 6000);
    }
  }

  $("#logoutAccount")?.addEventListener("click", logout);
}

async function main() {
  initBase();
  await loadProducts(location.pathname.includes("admin"));
  initAuth();
  initCatalog();
  initOrder();
  initQuickOrderForm();
  await initAdmin();
  initUserDashboard();
}

main();
