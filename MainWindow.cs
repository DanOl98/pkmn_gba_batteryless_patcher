namespace BatterylessPatcher
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void enableDisableControls(bool enable)
        {
            btnBrowse.Enabled = enable;
            //btnPatchBad.Enabled = enable;
            //btnPatchGood.Enabled = enable;
            btnPatchGood2.Enabled = enable;
            btnRepackOnly.Enabled = enable;
            repackCheck.Enabled = enable;
            //outputText.Enabled = enable;
        }
        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            if (ofd.ShowDialog(this) == DialogResult.OK)
                txtRomPath.Text = ofd.FileName;
        }

        private void BtnPatchGood_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRomPath.Text) || !File.Exists(txtRomPath.Text))
            {
                MessageBox.Show(this, "First select a valid GBA ROM file", "Attenzione",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            enableDisableControls(false);
            new Thread(() =>
            {
                //Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Priority = ThreadPriority.Highest;

                try
                {


                    byte[] rom = File.ReadAllBytes(txtRomPath.Text);

                    var result = PatcherRSITA.ApplyBatterylessPatch(
        rom,
        repackCheck.Checked
    );

                    string outPath = Path.Combine(
                        Path.GetDirectoryName(txtRomPath.Text)!,
                        Path.GetFileNameWithoutExtension(txtRomPath.Text) + ".batteryless.gba"
                    );
                    File.WriteAllBytes(outPath, result);
                    Console.WriteLine($"[INFO] Saved in {outPath}");

                    this.Invoke(new Action(() => MessageBox.Show(this, $"Patch applied!\nSaved as:\n{outPath}",
                        "OK", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    this.Invoke(new Action(() => enableDisableControls(true)));

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERR] Message: {ex.Message}");
                    Console.WriteLine($"[ERR] Stacktrace: {ex.StackTrace}");
                    this.Invoke(new Action(() => MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)));

                }
            }).Start();
        }


        private void BtnPatchBad_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRomPath.Text) || !File.Exists(txtRomPath.Text))
            {
                MessageBox.Show(this, "First select a valid GBA ROM file", "Attenzione",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            enableDisableControls(false);
            new Thread(() =>
            {

                try
                {


                    byte[] rom = File.ReadAllBytes(txtRomPath.Text);

                    var result = PatcherRSUSA.ApplyBatterylessPatch(
        rom,
        repackCheck.Checked
    );

                    string outPath = Path.Combine(
                        Path.GetDirectoryName(txtRomPath.Text)!,
                        Path.GetFileNameWithoutExtension(txtRomPath.Text) + ".batteryless.gba"
                    );
                    File.WriteAllBytes(outPath, result);
                    Console.WriteLine($"[INFO] Saved in {outPath}");

                    this.Invoke(new Action(() => MessageBox.Show(this, $"Patch applied!\nSaved as:\n{outPath}",
                        "OK", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    this.Invoke(new Action(() => enableDisableControls(true)));

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERR] Message: {ex.Message}");
                    Console.WriteLine($"[ERR] Stacktrace: {ex.StackTrace}");
                    this.Invoke(new Action(() => MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)));

                }
            }).Start();
        }

        private void BtnPatchGood2_Click(object? sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtRomPath.Text) || !File.Exists(txtRomPath.Text))
            {
                MessageBox.Show(this, "First select a valid GBA ROM file", "Attenzione",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            enableDisableControls(false);
            new Thread(() =>
            {

                try
                {


                    byte[] rom = File.ReadAllBytes(txtRomPath.Text);

                    var result = PatcherFRLGITA.ApplyBatterylessPatch(
        rom,
        repackCheck.Checked
    );

                    string outPath = Path.Combine(
                        Path.GetDirectoryName(txtRomPath.Text)!,
                        Path.GetFileNameWithoutExtension(txtRomPath.Text) + ".batteryless.gba"
                    );
                    File.WriteAllBytes(outPath, result);
                    Console.WriteLine($"[INFO] Saved in {outPath}");

                    this.Invoke(new Action(() => MessageBox.Show(this, $"Patch applied!\nSaved as:\n{outPath}",
                        "OK", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    this.Invoke(new Action(() => enableDisableControls(true)));

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERR] Message: {ex.Message}");
                    Console.WriteLine($"[ERR] Stacktrace: {ex.StackTrace}");
                    this.Invoke(new Action(() => MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)));

                }
            }).Start();
        }

        private void BtnRepackOnly_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRomPath.Text) || !File.Exists(txtRomPath.Text))
            {
                MessageBox.Show(this, "First select a valid GBA ROM file", "Attenzione",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            enableDisableControls(false);
            new Thread(() =>
            {

                try
                {


                    byte[] rom = File.ReadAllBytes(txtRomPath.Text);

                    var result = Repack.repack(rom, new() { });

                    string outPath = Path.Combine(
                        Path.GetDirectoryName(txtRomPath.Text)!,
                        Path.GetFileNameWithoutExtension(txtRomPath.Text) + ".repacked.gba"
                    );
                    File.WriteAllBytes(outPath, result);

                    Console.WriteLine($"[INFO] Saved in {outPath}");

                    this.Invoke(new Action(() => MessageBox.Show(this, $"Repacked!\nSaved as:\n{outPath}",
                        "OK", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    this.Invoke(new Action(() => enableDisableControls(true)));

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERR] Message: {ex.Message}");
                    Console.WriteLine($"[ERR] Stacktrace: {ex.StackTrace}");
                    this.Invoke(new Action(() => MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)));

                }
            }).Start();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Console.SetOut(new ControlWriter(outputText));
            Console.WriteLine("POKEMON GBA PATCHER FOR BOOTLEG CARTRIDGES");
        }

        private void outputText_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
