import type { Metadata } from "next";
import { Header } from "@/components/navigation/Header";
import "./globals.css";

export const metadata: Metadata = {
  title: "Nibelung Logs",
  description: "World of Warcraft raid logs analyzer",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ru" className="dark">
      <body className="antialiased min-h-screen bg-background">
        <Header />
        <main className="min-h-[calc(100vh-5rem)]">
          {children}
        </main>
      </body>
    </html>
  );
}
