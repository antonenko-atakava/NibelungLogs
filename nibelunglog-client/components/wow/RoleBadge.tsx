"use client";

import Image from "next/image";
import { cn } from "@/lib/utils";

interface RoleBadgeProps {
  role: string | null | undefined;
  children?: React.ReactNode;
}

const roleIconMap: Record<string, string> = {
  "0": "/images/wow/roles/dps.png",
  "1": "/images/wow/roles/tank.png",
  "2": "/images/wow/roles/healer.png",
  "Tank": "/images/wow/roles/tank.png",
  "Healer": "/images/wow/roles/healer.png",
  "DPS": "/images/wow/roles/dps.png",
};

export function RoleBadge({ role }: RoleBadgeProps) {
  if (!role)
    return <span className="text-muted-foreground">-</span>;

  const iconPath = roleIconMap[role];

  if (!iconPath)
    return <span className="text-muted-foreground">-</span>;

  return (
    <div className="flex items-center justify-center">
      <Image
        src={iconPath}
        alt={role}
        width={20}
        height={20}
        className="object-contain"
      />
    </div>
  );
}
