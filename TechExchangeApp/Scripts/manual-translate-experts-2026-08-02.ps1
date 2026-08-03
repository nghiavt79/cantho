$ErrorActionPreference = "Stop"

$connectionString = "Server=localhost;Database=TechExchangeNew;User ID=sa;Password=111111;TrustServerCertificate=True;"

function New-Sha256Hash([string[]]$Parts) {
    $raw = [string]::Join([char]0x241F, ($Parts | ForEach-Object { if ($null -eq $_) { "" } else { $_ } }))
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($raw)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { $hash = $sha.ComputeHash($bytes) }
    finally { $sha.Dispose() }
    -join ($hash | ForEach-Object { $_.ToString("X2") })
}

function New-Slug([string]$Text) {
    if ([string]::IsNullOrWhiteSpace($Text)) { return "" }
    $normalized = $Text.Normalize([Text.NormalizationForm]::FormD)
    $sb = [System.Text.StringBuilder]::new()
    foreach ($ch in $normalized.ToCharArray()) {
        if ([Globalization.CharUnicodeInfo]::GetUnicodeCategory($ch) -ne [Globalization.UnicodeCategory]::NonSpacingMark) {
            [void]$sb.Append($ch)
        }
    }
    $s = $sb.ToString().Normalize([Text.NormalizationForm]::FormC).ToLowerInvariant()
    $s = [Text.RegularExpressions.Regex]::Replace($s, "[^a-z0-9\s-]", "")
    $s = [Text.RegularExpressions.Regex]::Replace($s, "\s+", "-")
    $s = [Text.RegularExpressions.Regex]::Replace($s, "-{2,}", "-").Trim("-")
    return $s
}

function Convert-ExpertHtml([string]$Text) {
    if ([string]::IsNullOrWhiteSpace($Text)) { return $Text }
    $replacements = [ordered]@{
        "Bậc" = "Degree"
        "Nơi đào tạo" = "Training institution"
        "Chuyên ngành" = "Major"
        "Nước" = "Country"
        "Năm" = "Year"
        "Thời gian" = "Period"
        "Nơi công tác" = "Organization"
        "Công việc" = "Role"
        "Vị trí" = "Position"
        "Cơ quan" = "Issuing agency"
        "Tổ chức" = "Organization"
        "Chức vụ" = "Position"
        "Nội dung" = "Content"
        "Số lượng" = "Quantity"
        "Giai đoạn" = "Period"
        "Sách tiêu biểu" = "Representative books"
        "Sách/giáo trình" = "Books/textbooks"
        "Giáo trình" = "Textbook"
        "Nhà xuất bản" = "Publisher"
        "Bài báo tiêu biểu" = "Representative publications"
        "Bài báo" = "Publication"
        "Tạp chí" = "Journal"
        "Tên sáng chế" = "Patent/Invention name"
        "Cơ quan cấp" = "Issuing agency"
        "Ngày cấp" = "Grant date"
        "Năm cấp" = "Grant year"
        "Đề tài/dự án" = "Research topic/project"
        "Đề tài/công nghệ" = "Research topic/technology"
        "Đề tài tiêu biểu" = "Representative research topics"
        "Đề tài" = "Research topic"
        "Cấp" = "Level"
        "Vai trò" = "Role"
        "Kết quả" = "Result"
        "Đại học" = "Bachelor"
        "Thạc sĩ" = "Master"
        "Tiến sĩ" = "Doctorate"
        "Giảng viên cao cấp" = "Senior lecturer"
        "Giảng viên chính" = "Principal lecturer"
        "Giảng viên" = "Lecturer"
        "Nghiên cứu viên cao cấp" = "Senior researcher"
        "Nghiên cứu viên" = "Researcher"
        "Trưởng bộ môn" = "Department head"
        "Phó trưởng bộ môn" = "Deputy department head"
        "Trưởng khoa" = "Dean"
        "Phó trưởng khoa" = "Deputy dean"
        "Viện trưởng" = "Director"
        "Phó Viện Trưởng" = "Deputy director"
        "Giám đốc" = "Director"
        "Phó giám đốc" = "Deputy director"
        "Phó chủ tịch Hội đồng trường" = "Vice Chair of the University Council"
        "Phó Chủ tịch" = "Vice Chair"
        "Trưởng phòng" = "Head of department"
        "Phó trưởng phòng" = "Deputy head of department"
        "Chuyên viên" = "Specialist"
        "Nhân viên" = "Staff"
        "Chủ trì" = "Principal investigator"
        "Đồng chủ trì" = "Co-principal investigator"
        "Tham gia" = "Participant"
        "Đã nghiệm thu, xếp loại tốt" = "Accepted, rated good"
        "Đã nghiệm thu" = "Accepted"
        "Xuất sắc" = "Excellent"
        "Tốt" = "Good"
        "Khá" = "Fair"
        "Đạt" = "Passed"
        "Ứng dụng thực tiễn" = "Practical application"
        "Cấp trường" = "University level"
        "Cấp Bộ" = "Ministerial level"
        "Cấp tỉnh" = "Provincial level"
        "Cấp cơ sở" = "Institutional level"
        "Nhà nước" = "National level"
        "Quốc tế" = "International"
        "trong nước" = "domestic"
        "Việt Nam" = "Vietnam"
        "CHLB Nga" = "Russian Federation"
        "CHLB Đức" = "Germany"
        "Hàn Quốc" = "South Korea"
        "Nhật Bản" = "Japan"
        "Hà Lan" = "Netherlands"
        "Úc" = "Australia"
        "Anh" = "United Kingdom"
        "Bỉ" = "Belgium"
        "Công nghệ thực phẩm" = "Food technology"
        "Bảo quản và Chế biến thực phẩm" = "Food preservation and processing"
        "Điện tử vô tuyến" = "Radio electronics"
        "Điện tử" = "Electronics"
        "Tự động hóa" = "Automation"
        "Thiết Kế Đồ Hoạ - Quảng Cáo" = "Graphic design and advertising"
        "Mỹ Thuật Ứng Dụng" = "Applied arts"
        "Công nghệ Thực phẩm và Đồ uống" = "Food and beverage technology"
        "Khoa học Thực phẩm" = "Food science"
        "Sinh học" = "Biology"
        "Di truyền chọn giống" = "Genetics and breeding"
        "Trồng trọt" = "Crop science"
        "Vi sinh vật" = "Microbiology"
        "Kỹ thuật Cơ khí" = "Mechanical engineering"
        "Cơ điện tử" = "Mechatronics"
        "Sư phạm Hóa học" = "Chemistry education"
        "Công nghệ và Quản lý môi trường" = "Environmental technology and management"
        "Kỹ thuật môi trường xây dựng" = "Built environmental engineering"
        "Tài chính tiền tệ" = "Finance and monetary economics"
        "Quản trị kinh doanh" = "Business administration"
        "Quản lý kinh tế" = "Economic management"
        "Kinh tế thương mại" = "Commercial economics"
        "Kinh tế phát triển" = "Development economics"
        "Kinh tế quốc tế" = "International economics"
        "Sách/chương sách quốc tế" = "International books/book chapters"
        "Bài báo quốc tế ISI/Scopus" = "International ISI/Scopus publications"
        "bài báo khoa học" = "scientific publications"
        "bằng sáng chế" = "patents"
        "bằng độc quyền sáng chế" = "patents"
        "giải pháp hữu ích" = "utility solutions"
        "và các bài báo khác" = "and other publications"
        "và các bài báo trong nước khác" = "and other domestic publications"
        "và nhóm bài báo khác" = "and other publication groups"
        "và các sách/giáo trình khác" = "and other books/textbooks"
        "và 3 đầu sách khác" = "and 3 other books"
        "tổng cộng" = "total"
        "từ 1993 đến nay" = "from 1993 to present"
        "5 năm gần đây" = "in the last 5 years"
        "nay" = "present"
    }
    $out = $Text
    foreach ($key in $replacements.Keys) {
        $out = $out.Replace($key, $replacements[$key])
    }
    return $out
}

function Invoke-Scalar($Conn, [string]$Sql, [hashtable]$Params = @{}) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = $Sql
    foreach ($k in $Params.Keys) {
        [void]$cmd.Parameters.AddWithValue($k, $(if ($null -eq $Params[$k]) { [DBNull]::Value } else { $Params[$k] }))
    }
    return $cmd.ExecuteScalar()
}

function Get-Row($Conn, [string]$Sql, [hashtable]$Params) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = $Sql
    foreach ($k in $Params.Keys) {
        [void]$cmd.Parameters.AddWithValue($k, $(if ($null -eq $Params[$k]) { [DBNull]::Value } else { $Params[$k] }))
    }
    $reader = $cmd.ExecuteReader()
    try {
        if (-not $reader.Read()) { return $null }
        $row = @{}
        for ($i = 0; $i -lt $reader.FieldCount; $i++) {
            $name = $reader.GetName($i)
            $row[$name] = if ($reader.IsDBNull($i)) { $null } else { $reader.GetValue($i) }
        }
        return $row
    }
    finally { $reader.Close() }
}

function Copy-ExpertWithOverrides($Conn, [int]$Id, [hashtable]$Overrides) {
    $exists = Invoke-Scalar $Conn "SELECT COUNT(1) FROM dbo.NhaTuVan WHERE OriginalId=@id AND LanguageId=2" @{ "@id" = $Id }
    if ([int]$exists -gt 0) { return $false }

    $source = Get-Row $Conn "SELECT * FROM dbo.NhaTuVan WHERE TuVanId=@id AND ISNULL(LanguageId,1)=1" @{ "@id" = $Id }
    if ($null -eq $source) { throw "Source expert not found: $Id" }

    $columns = @()
    $values = @()
    foreach ($name in $source.Keys | Sort-Object) {
        if ($name -eq "TuVanId") { continue }
        $columns += "[$name]"
        $values += "@$name"
    }

    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = "INSERT INTO dbo.NhaTuVan ($([string]::Join(',', $columns))) VALUES ($([string]::Join(',', $values)));"
    foreach ($name in $source.Keys | Sort-Object) {
        if ($name -eq "TuVanId") { continue }
        $value = if ($Overrides.ContainsKey($name)) { $Overrides[$name] } else { $source[$name] }
        [void]$cmd.Parameters.AddWithValue("@$name", $(if ($null -eq $value) { [DBNull]::Value } else { $value }))
    }
    [void]$cmd.ExecuteNonQuery()
    return $true
}

$expertText = @{
    4124 = @{
        HocHam = "Professor"
        ChucVu = "Senior Lecturer"
        DichVu = "Consulting on food preservation and processing technology, and building production processes for agricultural and food enterprises."
        KetQuaNghienCuu = "Principal investigator/participant in 25 research topics (1997-2024), author of 191 scientific publications (145 domestic, 46 international WoS/Scopus) and 8 books/textbooks."
    }
    4132 = @{
        HocHam = "Associate Professor"
        ChucVu = "Vice Chair of the University Council"
        DichVu = "Consulting on automation, intelligent control, IoT, machine vision, and AI applications in agriculture and industry."
        KetQuaNghienCuu = "Principal investigator/participant in 3 research topics/projects (2015-2021), author of 15 international journal papers, 5 international conference papers and 8 national conference papers (2020-2024), and holder of 2 patents."
    }
    4544 = @{
        HocHam = $null
        ChucVu = "Dean of Design; Director"
        DichVu = "Consulting on graphic and advertising design, and AR/VR technology applications in applied arts and communications."
        KetQuaNghienCuu = "Visiting lecturer at multiple universities and colleges; participated in producing many television programs for Hau Giang Television and VTV. The profile does not list scientific publications or patents."
    }
    4545 = @{
        HocHam = $null
        ChucVu = "Lecturer"
        DichVu = "Consulting on OCOP product processing processes (tea and fermented fruit beverages), and physicochemical food analysis."
        KetQuaNghienCuu = "Principal investigator/participant in 6 research topics (2006-2024), author of about 30 international ISI/Scopus publications, 3 international books/book chapters and 2 patents."
    }
    4546 = @{
        HocHam = "Professor"
        ChucVu = "Director"
        DichVu = "Consulting on rice breeding (salinity, drought and flood tolerance, high quality traits), and molecular marker applications in crop breeding."
        KetQuaNghienCuu = "Principal investigator/participant in 157 research topics/projects (1993-2026), author of more than 437 scientific publications and 24 books/textbooks, holder of 4 invention patents and 20 utility solution certificates for rice varieties."
    }
    4547 = @{
        HocHam = "Associate Professor"
        ChucVu = "Vice Chair"
        DichVu = "Consulting on isolation and selection of microorganisms (nitrogen-fixing, endophytic and antibacterial) for agriculture and environmental treatment."
        KetQuaNghienCuu = "Principal investigator/participant in 13 research topics (1985-2017), author of 19 scientific publications (6 international, 13 domestic) and 5 books/textbooks."
    }
    4548 = @{
        HocHam = $null
        ChucVu = "Vice Rector"
        DichVu = "Consulting on cybersecurity, computer virus/malware prevention, and digital transformation for organizations and enterprises."
        KetQuaNghienCuu = "Principal investigator/participant in 4 research topics (2004-2023), author of 8 scientific publications (1997-2023) and 2 books/textbooks on information technology."
    }
    4549 = @{
        HocHam = "Associate Professor"
        ChucVu = "Director"
        DichVu = "Consulting on mechanical design, mechatronics, industrial and medical robotics, and automatic control."
        KetQuaNghienCuu = "Principal investigator of 20 research topics/projects (2002-2022), author of 20 international journal papers and 34 domestic journal papers in the last 5 years, and 2 textbooks."
    }
    4550 = @{
        HocHam = $null
        ChucVu = "Deputy Dean of the Faculty of Environment, Institute for Sustainable Development Research"
        DichVu = "Consulting on wastewater treatment technology, adsorbent materials for water pollution treatment, and environmental technology valuation."
        KetQuaNghienCuu = "Principal investigator of 3 research topics (2021-2023), author of 5 scientific publications (2024) on adsorbent materials for water treatment, and 4 technologies applied in practice (2013-2022)."
    }
    4551 = @{
        HocHam = $null
        ChucVu = "Director of the Consulting Center for Science Evaluation and Technology Valuation"
        DichVu = "Consulting on technology valuation, enterprise technology capability assessment, intellectual property valuation and technology transfer."
        KetQuaNghienCuu = "Principal investigator of 6 ministerial/provincial research topics (2017-2023) and participant in many other topics; author of 6 scientific publications (2022-2024) on technology valuation."
    }
    4552 = @{
        HocHam = "Associate Professor"
        ChucVu = "Dean"
        DichVu = "Consulting on international economics, export supply chains and value chains, and policies for attracting foreign direct investment (FDI)."
        KetQuaNghienCuu = "Principal investigator/participant in 7 research topics (2010-2024), author of 38 scientific publications (2006-2024, including many Q1/Q2/Q3 papers in Sustainability, Journal of the Asia Pacific Economy, and related journals) and 8 books/textbooks."
    }
}

$sourceHashFields = @("HocHam","ChucVu","DichVu","KetQuaNghienCuu","QuaTrinhCongTac","CongBoKhoaHoc")
$htmlFields = @("QuaTrinhDaoTao","QuaTrinhCongTac","CongBoKhoaHoc","SangChe","DuAnNghienCuu","KinhNghiem")

$conn = [System.Data.SqlClient.SqlConnection]::new($connectionString)
$conn.Open()
try {
    $created = 0
    foreach ($id in ($expertText.Keys | Sort-Object)) {
        $source = Get-Row $conn "SELECT * FROM dbo.NhaTuVan WHERE TuVanId=@id AND ISNULL(LanguageId,1)=1" @{ "@id" = [int]$id }
        if ($null -eq $source) { continue }

        $overrides = @{
            LanguageId = 2
            OriginalId = [int]$id
            EnStale = $false
            StatusId = 3
            Created = [DateTime]::Now
            CreatedBy = "codex"
            Modified = [DateTime]::Now
            Modifier = "codex"
            QueryString = New-Slug ([string]$source["FullName"])
            SourceHash = New-Sha256Hash ($sourceHashFields | ForEach-Object { [string]$source[$_] })
        }

        foreach ($k in $expertText[$id].Keys) { $overrides[$k] = $expertText[$id][$k] }
        foreach ($field in $htmlFields) {
            $overrides[$field] = Convert-ExpertHtml ([string]$source[$field])
        }

        if (Copy-ExpertWithOverrides $conn ([int]$id) $overrides) { $created++ }
    }
    "Created expert translations: $created"
}
finally {
    $conn.Close()
}
