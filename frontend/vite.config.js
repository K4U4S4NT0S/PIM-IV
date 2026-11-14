import { resolve } from "path";
import { defineConfig } from "vite";

export default defineConfig({
  build: {
    rollupOptions: {
      input: {
        index: resolve(__dirname, "index.html"),
        cardapio: resolve(__dirname, "cardapio.html"),
        pedido: resolve(__dirname, "pedido.html"),
        contato: resolve(__dirname, "contato.html"),
        login: resolve(__dirname, "login.html"),
        cadastro: resolve(__dirname, "cadastro.html"),
        produto: resolve(__dirname, "produto.html"),
        admin: resolve(__dirname, "admin.html"),
        usuario: resolve(__dirname, "usuario.html")
      }
    }
  }
});
