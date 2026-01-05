export default function PlayersLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="fixed inset-0 top-0">
      {children}
    </div>
  );
}

